using MediatR;

namespace PrettyPrintClipboardPls
{
	public class PrettyPrintCommand : IRequest<string>
	{
		public string Text { get; set; }
	}
}