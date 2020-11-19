using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MasterDevs.ChromeDevTools.Messages.Protocol.Chrome;
using MasterDevs.ChromeDevTools.Protocol.Chrome.DOM;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Page;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Runtime;
using MasterDevs.ChromeDevTools.Protocol.Chrome.Target;
using Newtonsoft.Json;

namespace MasterDevs.ChromeDevTools.Messages {
	internal class Program {
		const int ViewPortWidth = 1440;
		const int ViewPortHeight = 900;

		private static void Main(string[] args) {
			Task.Run(async () => {
				// synchronization
				var screenshotDone = new ManualResetEventSlim();

				// STEP 1 - Run Chrome
				var chromeProcessFactory = new ChromeProcessFactory(new StubbornDirectoryCleaner());
				using (var chromeProcess = chromeProcessFactory.Create(9222, false)) {
					// STEP 2 - Create a debugging session
					var sessionInfo = (await chromeProcess.GetSessionInfo()).LastOrDefault();
					var chromeSessionFactory = new CustomSessionFactory();
					var chromeSession = chromeSessionFactory.Create(sessionInfo.WebSocketDebuggerUrl);

					((ChromeMessageSession)chromeSession).SubscribeFinal(dynObj => {
						Console.WriteLine("Received generic object");
						Console.WriteLine(JsonConvert.SerializeObject(dynObj));
					});
					
					chromeSession.Subscribe<BindingCalledEvent>(bindingEvent => {
						Console.WriteLine("Binding event called");
					});
					chromeSession.Subscribe<ExecutionContextCreatedEvent>(evt => {
						// we cannot block in event handler, hence the task
						Task.Run(async () => {
							Console.WriteLine("execution event received: " + DateTime.Now);
							//Attach to the target
							var tgtResp = await chromeSession.SendAsync(new AddBindingCommand{ name = "selfie", executionContextId = evt.Context.Id});
							Console.WriteLine("Bound to new execution context");
						});
					});
					chromeSession.Subscribe<InspectRequestedEvent>(inspectEvent => {
						// we cannot block in event handler, hence the task
						Task.Run(async () => {
							Console.WriteLine("inspect event received: " + DateTime.Now);

						});
					});
					
					// STEP 3 - Send a command
					//
					// Here we are sending a commands to tell chrome to set the viewport size 
					// and navigate to the specified URL
					await chromeSession.SendAsync(new SetDeviceMetricsOverrideCommand {
						Width = ViewPortWidth,
						Height = ViewPortHeight,
						Scale = 1
					});

					var navigateResponse = await chromeSession.SendAsync(new NavigateCommand {
						Url = "https://www.google.com"
					});
					Console.WriteLine("NavigateResponse: " + navigateResponse.Id);

					// STEP 4 - Register for events (in this case, "Page" domain events)
					// send an command to tell chrome to send us all Page events
					// but we only subscribe to certain events in this session
					var runtimeResult = await chromeSession.SendAsync<MasterDevs.ChromeDevTools.Protocol.Chrome.Runtime.EnableCommand>();
					var targetResp = await chromeSession.SendAsync(new GetTargetsCommand());
					string tgtId = null;
					foreach (var tgt in targetResp.Result.TargetInfos) {
						if (tgt.Type.Equals("page", StringComparison.InvariantCultureIgnoreCase)) {
							var tgtAttach = await chromeSession.SendAsync(new AttachToTargetCommand { TargetId = tgt.TargetId });
							Console.WriteLine($"Target {tgt.TargetId} attached: {tgtAttach.Result.SessionId}");
							tgtId = tgt.TargetId;
						}
					}

					
					var bindResp = await chromeSession.SendAsync(new AddBindingCommand{ name = "selfie" });
					Console.WriteLine("Bound to new execution context");

					// wait for screenshoting thread to (start and) finish
					screenshotDone.Wait();

					Console.WriteLine("Exiting ..");
				}
			}).Wait();
		}
	}
}