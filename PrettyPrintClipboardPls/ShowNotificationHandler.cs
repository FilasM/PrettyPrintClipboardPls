using MediatR;
using WPFNotification.Model;
using WPFNotification.Services;

namespace PrettyPrintClipboardPls
{
    public class ShowNotificationHandler : IRequestHandler<ShowNotificationCommand>
	{
		private readonly INotificationDialogService _notificationService;

		public ShowNotificationHandler(INotificationDialogService notificationService)
		{
			_notificationService = notificationService;
		}

		public void Handle(ShowNotificationCommand message)
		{
		    var notification = new Notification()
		    {
		        Title = "Pretty Print Clipboard Pls",
		        Message = message.Message
		    };
			this._notificationService.ShowNotificationWindow(notification);
		}
	}
}