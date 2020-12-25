using MediatR;

namespace PrettyPrintClipboardPls
{
	public class ShowNotificationCommand : IRequest
	{
	    public string Message { get; set; }
	}
}