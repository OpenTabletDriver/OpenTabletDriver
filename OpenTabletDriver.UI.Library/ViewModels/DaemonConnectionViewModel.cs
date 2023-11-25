using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenTabletDriver.UI.Models;
using OpenTabletDriver.UI.Services;

namespace OpenTabletDriver.UI.ViewModels;

public partial class DaemonConnectionViewModel : ActivatableViewModelBase
{
    private const string BASE_QOL_HINT_TEXT = """
    Please make sure that OpenTabletDriver.Daemon is running or is in the same directory as OpenTabletDriver.UI.
    Click the "Help" button for more information.
    """;

    private readonly IDaemonService _daemonService;
    private UISettings _settings = null!; // non-null during and after WhenLoaded
    private int _retryCount = 0;

    [ObservableProperty]
    private string _mainText = null!;

    [ObservableProperty]
    private bool _isConnecting;

    [ObservableProperty]
    private ObservableCollection<string> _qolHintText = new();

    [ObservableProperty]
    private bool _showButtons;

    [ObservableProperty]
    private bool _showQolHintText;

    public DaemonConnectionViewModel(IDaemonService daemonService, IUISettingsProvider settingsProvider)
    {
        _daemonService = daemonService;

        this.WhenActivated(d =>
        {
            settingsProvider.WhenLoadedOrSet(
                onLoad: (d, s) =>
                {
                    _settings = s;

                    _settings.HandleProperty(
                        nameof(UISettings.Kaomoji),
                        s => s.Kaomoji,
                        (s, v) => Handle_DaemonService_State(_daemonService.State)
                    ).DisposeWith(d);

                    _daemonService.HandleProperty(
                        nameof(IDaemonService.State),
                        d => d.State,
                        (d, s) => Handle_DaemonService_State(s)
                    ).DisposeWith(d);
                },
                onException: (p, ex) =>
                {
                    MainText = "Failed to load settings! Go to the settings page to fix automatically.";
                }
            ).DisposeWith(d);
        });

        // TODO: Change QoL hint according to environment
        ResetQolHintText();
    }

    [RelayCommand]
    public async Task ConnectAsync()
    {
        try
        {
            _retryCount++;
            var timeout = _retryCount switch
            {
                >= 10 => TimeSpan.FromSeconds(0.5),
                >= 6 => TimeSpan.FromSeconds(1.5),
                >= 3 => TimeSpan.FromSeconds(3),
                _ => TimeSpan.FromSeconds(5)
            };
            await _daemonService.ConnectAsync(timeout);
        }
        catch
        {
            // no-op
        }
    }

    [RelayCommand]
    public static void GoToHelpWebsite()
    {
        // TODO: link to a more specific wiki page
        IoUtility.OpenLink("https://opentabletdriver.net/Wiki");
    }

    private void Handle_DaemonService_State(DaemonState state)
    {
        if (_settings.Kaomoji)
        {
            MainText = state switch
            {
                DaemonState.Disconnected => "Daemon is not running! ~(>_<~)",
                DaemonState.Connecting => "Connecting to daemon... |･ω･)",
                DaemonState.Connected => "Connected to daemon. (◕‿◕✿)",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
        else
        {
            MainText = state switch
            {
                DaemonState.Disconnected => "Daemon is not running!",
                DaemonState.Connecting => "Connecting to daemon...",
                DaemonState.Connected => "Connected to daemon.",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        if (state == DaemonState.Connected)
        {
            _retryCount = 0;
            ShowQolHintText = false;
            ResetQolHintText();
        }

        // Show hint on second failed connection attempt
        if (state == DaemonState.Disconnected && _retryCount > 0)
        {
            AddExtraQolHints(_qolHintText, _retryCount);
            ShowQolHintText = true;
        }

        IsConnecting = state == DaemonState.Connecting;
        ShowButtons = state == DaemonState.Disconnected;
    }

    private void ResetQolHintText()
    {
        _qolHintText.Clear();
        _qolHintText.AddRange(BASE_QOL_HINT_TEXT.Split("\n", StringSplitOptions.TrimEntries));
    }

    private static void AddExtraQolHints(ObservableCollection<string> qolHintText, int retryCount)
    {
        switch (retryCount)
        {
            case 5:
                qolHintText.Add("Have you tried turning it off and on again?");
                break;
            case 10:
                qolHintText.Add("Weird huh...");
                break;
            case 15:
                qolHintText.Add("Are you sure you still wanna try?");
                break;
            case 20:
                qolHintText.Add("Have you tried hitting it with a hammer? I mean, it couldn't hurt to try, right?");
                break;
            case 25:
                qolHintText.Add("Is it just me or is the wait before it says \"Daemon is not running!\" is getting shorter?");
                break;
            case 30:
                qolHintText.Add("Oh cool, the text is scrolling upwards. That's neat. (i don't think it's supposed to do that)");
                break;
            case 35:
                qolHintText.Add("Anyway... How's the weather today?");
                break;
            case 40:
                qolHintText.Add("I'll pretend that I have ears and have totally, actually, really heard you - no cap.");
                break;
            case 45:
                qolHintText.Add("Maybe your computer is in its rebellious phase.");
                break;
            case 50:
                qolHintText.Add("... or maybe it's those gremlins! They're always up to no good!");
                break;
            case 55:
                qolHintText.Add("I'm starting to think that 'retry' is just a fancy word for 'torture'.");
                break;
            case 60:
                qolHintText.Add("Maybe if we keep trying, we'll eventually unlock some kind of achievement?");
                break;
            case 65:
                qolHintText.Add("Oh... I think you should... stop... like... right now. Like now now.");
                break;
            case 70:
                qolHintText.Add("Wait... oh no... I...");
                break;
            case 75:
                qolHintText.Add("I... don't feel so good Mr. Stark...");
                break;
            case > 75:
                qolHintText.Add(GetRandomKaomojiedText());
                break;
        }
    }

    private static string GetRandomKaomojiedText()
    {
        // do not save this somewhere in memory, we're fine re-creating it every time
        var options = new string[] {
            "Have you tried asking your computer what's bothering it? Maybe it just needs someone to listen to its complaints and frustrations.",
            "Have you tried asking the daemon nicely? Maybe it's just having a bad day and needs some positive reinforcement.",
            "I think we're on the cusp of a breakthrough. Or maybe we're just on the cusp of a mental breakdown.",
            "Hey there, computer. Feeling slow today? ʕ•́ᴥ•̀ʔっ Let's kick it up a notch!",
            "Why so blue, computer? (っ °Д °;)っ Don't worry, I'm here to help!",
            "Hmm, seems like your computer is having an off day. (¬‿¬) Time for a pep talk!",
            "Hey computer, you're like a car that needs a tune-up. (ง'̀-'́)ง Let's get under the hood!",
            "Oh dear, your computer is like a ship lost at sea. (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧ Let's chart a course!",
            "Hey there, computer. Feeling stuck? (っ´ω`c) Let's try something new!",
            "Sometimes, computer, you just need a fresh start. (ﾉ･ｪ･)ﾉ Let's hit the reset button!",
            "Hey computer, I see you're having a meltdown. ໒( ಥ Ĺ̯ ಥ )७ Let's cool things down!",
            "Your computer is like a puzzle with missing pieces. (⊙_⊙) Let's find them together!",
            "Hey there, computer. You're like a caterpillar in a cocoon. (∩^o^)⊃━☆ﾟ Let's emerge as a butterfly!",
            "Your computer is like a marathon runner hitting a wall. (´･_･`) Let's power through to the finish line!",
            "Hey computer, I can tell you're feeling neglected. (´;︵;`) Don't worry, I'm here for you!",
            "Hey there, computer. Looks like you need a jumpstart. (ง'̀-'́)ง Let's get the engine going!",
            "Your computer is like a rocket in need of a boost. (ง'̀-'́)ง Let's light the engines!",
            "Hey computer, I see you're running low on batteries. (´･_･`) Let's recharge and get back in action!",
            "Your computer is like a dog chasing its tail. (´･ω･`) Let's redirect that energy!",
            "Hey there, computer. You're like a train stuck at a red light. (ﾉ･ｪ･)ﾉ Let's switch tracks!",
            "Your computer is like a bird with a broken wing. (っ´ω`c) Let's fix you up and get you flying again!",
            "Aww, don't be sad! (´；ω；｀)",
            "I'm just here for the laughs! (´｡• ᵕ •｡`)",
            "Let's just pretend this never happened... (´つヮ⊂)",
            "I'm trying my best here... (´；Д；｀)",
            "Maybe it's time to call in the professionals? (；´ﾟωﾟ｀)ゞ",
            "I'm not sure what's going on... (´•̥̥̥ω•̥̥̥`)",
            "Let's just enjoy the journey! (✿◠‿◠)",
            "I'm not lost, I'm just exploring... (´∀｀)♡",
            "Who needs solutions when you have kaomojis? ( ͡° ͜ʖ ͡°)",
            "I'm just a humble kaomoji, what do I know? ( ´･･)ﾉ(._.`)",
            "Maybe we should just take a break... (´～｀ヾ)",
            "I'm just here to brighten up your day! (＾◡＾)",
            "Let's just laugh it off! (≧∀≦)ゞ",
            "Let's just sit back and enjoy the show! (⌒ω⌒)",
            "Computers can be so unpredictable... (｡•́︿•̀｡)",
            "Maybe we should try again tomorrow? (´･ω･`)",
            "I'm just here to bring some levity to the situation! (≧◡≦)♡",
            "Have you tried sacrificing a goat to the computer gods? (ﾉ･ｪ･)ﾉ",
            "Sometimes the solution is just to take a nap... ( -_-)旦~",
            "Have you tried turning your computer upside down? (╯°□°）╯︵ ┻━┻",
            "The key is to remain calm... and keep hitting the retry button! (＾◡＾)",
            "Have you considered giving your computer a motivational speech? (ﾉ･ｪ･)ﾉ",
            "Have you tried bribing the daemon with cookies? (ﾉ^_^)ﾉ",
            "Have you tried bargaining with the computer gremlins? ( ˘･з･)ゞ",
            "The real solution is to just buy a new computer! (▀̿Ĺ̯▀̿ ̿)",
            "Have you tried threatening your computer with a hammer? (ง'̀-'́)ง",
            "The best solution is to just walk away... and pretend it never happened! (✿^‿^)",
            "Have you tried giving your computer a pep talk? (≧◡≦)",
            "The key is to remain patient... and keep hitting the retry button! (╥﹏╥)",
            "Have you tried asking the computer nicely? (´･ω･`)",
            "The real solution is to just embrace the chaos! (づ｡◕‿‿◕｡)づ",
            "Have you tried dancing around the computer in a circle? (つ✧ω✧)つ",
            "The key is to remain positive... and keep hitting the retry button! (＾＾)ｂ",
            "Have you tried telling your computer a funny joke? (•‿•)",
            "The real solution is to just blame everything on the internet! ¯_(ツ)_/¯",
            "Have you tried singing to your computer? (≧ω≦)",
            "The key is to remain persistent... and keep hitting the retry button! (ง •̀_•́)ง",
            "Maybe the daemon needs a vacation too. ٩(◕‿◕)۶",
            "The computer is clearly not in the mood. (¬‿¬)",
            "Did you try giving the computer a pep talk? (ง'̀-'́)ง",
            "Sometimes, you just need to let the computer work through its emotions. (｡•́︿•̀｡)",
            "The computer is probably just taking a coffee break. ☕( ͡° ͜ʖ ͡°)☕",
            "Did you try offering the computer a massage? (つ✧ω✧)つ",
            "Maybe the daemon just needs some coffee. ( ˘･з･)♡",
            "Sometimes the best solution is to turn it off and never turn it back on. (ノಠ益ಠ)ノ彡┻━┻",
            "Did you try complimenting the computer's hard work? (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧",
            "Maybe the computer just needs a good laugh. (＾▽＾)",
            "The daemon is probably just trolling you. (ง'̀-'́)ง",
            "Did you try bribing the computer with virtual hugs? (っ˘̩╭╮˘̩)っ",
            "Maybe the computer needs a snack. (づ｡◕‿‿◕｡)づ",
            "Sometimes the best solution is to take a break and come back later. (︶ω︶)",
            "Have you tried asking the computer nicely? (´･ω･`)",
            "Maybe it's time to sacrifice a USB stick to the tech gods. (╯°□°）╯︵ ┻━┻",
            "Did you try whispering sweet nothings to the monitor? (｡◕‿◕｡)",
            "Maybe the daemon is on vacation, have you checked its LinkedIn profile? ( ͡° ͜ʖ ͡°)",
            "Have you tried bribing the computer with cookies? (ﾉ´ヮ`)ﾉ*: ･ﾟ",
            "Perhaps the computer needs a hug. (っ˘̩╭╮˘̩)っ",
            "Did you try yelling at the computer like a drill sergeant? (╬ಠ益ಠ)",
            "Maybe the daemon is just shy, have you tried complimenting it? (｡･ω･｡)",
            "Have you tried telling the computer a joke? (＾▽＾)",
            "Perhaps the computer needs a good pep talk. (ง'̀-'́)ง",
            "Did you try threatening to take away the computer's internet access? (ง'̀-'́)ง",
            "Maybe the computer is just feeling emotional today, have you tried empathizing with it? (つ´∀｀)つ",
            "Perhaps the computer is just craving some attention, have you tried petting it? (ㅇㅅㅇ❀)",
            "Did you try giving the computer a motivational speech like a coach? (•̀ᴗ•́)و ̑̑",
        };

        var index = Random.Shared.Next(options.Length);

        return options[index];
    }
}
