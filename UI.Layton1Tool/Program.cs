using CrossCutting.Core.Contract.DependencyInjection;
using CrossCutting.Core.Contract.EventBrokerage;
using CrossCutting.Core.Contract.Messages;
using ImGui.Forms;
using ImGui.Forms.Localization;
using Layton1Tool;
using UI.Layton1Tool.Forms.Contract;
using UI.Layton1Tool.Resources.Contract;

KernelLoader loader = new();
ICoCoKernel kernel = loader.Initialize();

var eventBroker = kernel.Get<IEventBroker>();
eventBroker.Raise(new InitializeApplicationMessage());

var fontFactory = kernel.Get<IFontFactory>();
fontFactory.RegisterFonts();

var localizer = kernel.Get<ILocalizer>();
var app = new Application(localizer);

var formFactory = kernel.Get<IFormFactory>();

Form form = formFactory.CreateMainForm();
form.DefaultFont = fontFactory.GetApplicationFont(15);

app.Execute(form);