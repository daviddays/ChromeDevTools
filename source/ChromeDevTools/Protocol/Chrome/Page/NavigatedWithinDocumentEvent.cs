namespace MasterDevs.ChromeDevTools.Protocol.Chrome.Page {
	[Event(ProtocolName.Page.InternalNavigationEvent)]
	[SupportedBy("Chrome")]
	public class NavigatedWithinDocumentEvent {
		public string frameId { get; set; }
		public string url { get; set; }
	}
}