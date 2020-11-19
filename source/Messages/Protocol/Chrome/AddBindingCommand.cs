namespace MasterDevs.ChromeDevTools.Messages.Protocol.Chrome {
	[Command(ProtocolName.Runtime.AddBinding)]
	[SupportedBy("Chrome")]
	public class AddBindingCommand : ICommand<AddBindingCommandResponse> {
		public long executionContextId { get; set; }
		public string name { get; set; }
	}
}