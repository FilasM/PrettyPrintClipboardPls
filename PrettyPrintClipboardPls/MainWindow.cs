using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Autofac;
using Autofac.Features.Variance;
using MediatR;
using WPFNotification.Services;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using System.Reflection;

namespace PrettyPrintClipboardPls
{
    public partial class MainWindow
	{
		private IContainer m_container;
		private IMediator m_mediator;
		private bool m_isReceivingShortcuts = true;
		protected CopyPasteInterceptor m_copyPasteInterceptor;

		protected IMediator Mediator
		{
			get
			{
				if (this.m_mediator == null)
				{
					this.m_mediator = m_container.Resolve<IMediator>();
				}
				return this.m_mediator;
			}
		}

		private void InitializeApp()
		{
			//IoC
			var builder = new ContainerBuilder();

			builder.RegisterSource(new ContravariantRegistrationSource());
			builder.RegisterAssemblyTypes(typeof(IMediator).Assembly).AsImplementedInterfaces();
			builder.RegisterAssemblyTypes(typeof(MainWindow).Assembly).AsImplementedInterfaces();
			builder.RegisterAssemblyTypes(typeof(INotificationDialogService).Assembly).AsImplementedInterfaces();
			builder.Register<SingleInstanceFactory>(ctx =>
			{
				var c = ctx.Resolve<IComponentContext>();
				return t => c.Resolve(t);
			});
			builder.Register<MultiInstanceFactory>(ctx =>
			{
				var c = ctx.Resolve<IComponentContext>();
				return t => (IEnumerable<object>)c.Resolve(typeof(IEnumerable<>).MakeGenericType(t));
			});

			//Populate the container with services that were previously registered
			//builder.Populate(services);

			m_container = builder.Build();

			//Handlers
			button.Click += Button_Click;

			m_copyPasteInterceptor = new CopyPasteInterceptor();
			m_copyPasteInterceptor.CopyPasteReceived += onCopyPasteReceived;

			//Exception Handlers
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				var exception = (Exception)e.ExceptionObject;

				ShowErrorMessage(exception.Message);
			};

			// Tray Icon
			var assembly = Assembly.GetExecutingAssembly();
			var iconStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.trayIcon.ico");
			NotifyIcon ni = new NotifyIcon
			{
				Icon = new System.Drawing.Icon(iconStream),
				Visible = true
			};
			ni.Text = "Pretty Print Clipboard Pls";
			ni.ContextMenu = CreateNotifyIconContextMenu();

			ni.DoubleClick += (object sender, EventArgs args) =>
				{
					if (WindowState == WindowState.Normal)
					{
						HideInTray();
					}
					else
					{
						BringWindowToFront();
					}
				};

			HideInTray();

			Mediator.Send(new ShowNotificationCommand
			{
				Message = "Pretty Print Clipboard Pls is running in tray..."
			});
		}

		private void BringWindowToFront()
		{
			WindowState = WindowState.Normal;
			Show();
			Activate();
		}

		private void HideInTray()
		{
			Hide();
			WindowState = WindowState.Minimized; // Must be set, else win 10 doesnt bring it to front on activate() call
		}

		private ContextMenu CreateNotifyIconContextMenu()
		{
			var contextMenu1 = new ContextMenu();

			// Initialize contextMenu1

			var showItem = new MenuItem
			{
				Index = 0,
				Text = "Show"
			};
			showItem.Click += (sender, args) =>
			{
				BringWindowToFront();
			};

			var exitItem = new MenuItem
			{
				Index = 1,
				Text = "Exit"
			};
			exitItem.Click += (object sender, EventArgs args) =>
			{
				Close();
			};

			contextMenu1.MenuItems.AddRange(
						new MenuItem[] { showItem, exitItem }
			);

			return contextMenu1;
		}

		private void onCopyPasteReceived()
		{
			if (!m_isReceivingShortcuts)
			{
				return;
			}

			Application.Current.Dispatcher.InvokeAsync(() =>
			{
				try
				{
					var cpContent = Clipboard.GetText();

					var output = ProcessInput(cpContent);

					if (output == null)
						return;

					inputText.Text = cpContent;
					SetOutputText(output);

					var move = moveCheckbox.IsChecked ?? false;
					if (move)
					{
						var cursorPos = GetCursorPos();

						Left = cursorPos.X - this.Width / 2;

						if (cursorPos.Y - this.Height / 2 < 0)
						{
							this.Top = 0;
						}
						else
						{
							Top = cursorPos.Y - this.Height / 2;
						}

						Activate();
					}

					var toClipboard = this.toClipboard.IsChecked ?? false;
					if (toClipboard)
					{
						Clipboard.SetText(output);

						Mediator.Send(new ShowNotificationCommand
						{
							Message = "Pretty print successful!"
						});
					}
				}
				catch (Exception e)
				{
					ShowErrorMessage(e.Message);
				}
			});
		}

		private void SetOutputText(string output)
		{
			if (output != null)
			{
				outputText.Text = output;
			}
		}

		private void ShowErrorMessage(string msg)
		{
			this.m_isReceivingShortcuts = false;

			MessageBox.Show(this, msg);

			this.m_isReceivingShortcuts = true;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var output = ProcessInput(inputText.Text);
			SetOutputText(output);
		}

		private string ProcessInput(string input)
		{
			var command = new PrettyPrintCommand
			{
				Text = input
			};

			var result = Mediator.Send(command).Result;

			return result;
		}

		private Point GetCursorPos()
		{
			var command = new GetCursorPosCommand();

			var result = Mediator.Send(command).Result;

			return result;
		}

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            this.m_copyPasteInterceptor.Dispose();
        }
    }
}