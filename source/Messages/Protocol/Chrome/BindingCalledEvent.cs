namespace MasterDevs.ChromeDevTools.Messages.Protocol.Chrome {
	[Event(ProtocolName.Runtime.BindingCalledEvent)]
	[SupportedBy("Chrome")]
	public class BindingCalledEvent {
		public string name { get; set; }
		public int executionContextId { get; set; }
		public string payload { get; set; }
	}
}