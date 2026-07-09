using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenTabletDriver.Desktop;

namespace OpenTabletDriver.UX
{
    public static class Strings
    {
        private static string? _language = null;
        private static readonly string _langFile = Path.Combine(AppInfo.Current.AppDataDirectory, "language.txt");

        static Strings()
        {
            try
            {
                if (System.IO.File.Exists(_langFile))
                    _language = System.IO.File.ReadAllText(_langFile).Trim();
            }
            catch { }
        }

        public static string Language
        {
            get => _language ?? (CultureInfo.CurrentUICulture.Name.StartsWith("zh") ? "zh" : "en");
            set
            {
                _language = value;
                try { System.IO.File.WriteAllText(_langFile, value); } catch { }
                OnLanguageChanged?.Invoke();
            }
        }

        public static event Action? OnLanguageChanged;

        public static string Output => _dict["Output"][Language];
        public static string Filters => _dict["Filters"][Language];
        public static string PenSettings => _dict["PenSettings"][Language];
        public static string AuxiliarySettings => _dict["AuxiliarySettings"][Language];
        public static string MouseSettings => _dict["MouseSettings"][Language];
        public static string Tools => _dict["Tools"][Language];
        public static string Info => _dict["Info"][Language];
        public static string Console => _dict["Console"][Language];
        public static string Tablet => _dict["Tablet"][Language];
        public static string Plugins => _dict["Plugins"][Language];
        public static string Presets => _dict["Presets"][Language];
        public static string TipSettings => _dict["TipSettings"][Language];
        public static string TipBinding => _dict["TipBinding"][Language];
        public static string TipThreshold => _dict["TipThreshold"][Language];
        public static string EraserSettings => _dict["EraserSettings"][Language];
        public static string EraserBinding => _dict["EraserBinding"][Language];
        public static string EraserThreshold => _dict["EraserThreshold"][Language];
        public static string PenButtons => _dict["PenButtons"][Language];
        public static string DisablePressure => _dict["DisablePressure"][Language];
        public static string DisableTilt => _dict["DisableTilt"][Language];
        public static string DragBindings => _dict["DragBindings"][Language];
        public static string Miscellaneous => _dict["Miscellaneous"][Language];
        public static string Auxiliary => _dict["Auxiliary"][Language];
        public static string WheelButtons => _dict["WheelButtons"][Language];
        public static string MouseButtons => _dict["MouseButtons"][Language];
        public static string MouseScrollwheel => _dict["MouseScrollwheel"][Language];
        public static string ScrollUp => _dict["ScrollUp"][Language];
        public static string ScrollDown => _dict["ScrollDown"][Language];
        public static string ClockwiseRotation => _dict["ClockwiseRotation"][Language];
        public static string ClockwiseRotationSettings => _dict["ClockwiseRotationSettings"][Language];
        public static string ClockwiseRotationThreshold => _dict["ClockwiseRotationThreshold"][Language];
        public static string CounterClockwiseRotation => _dict["CounterClockwiseRotation"][Language];
        public static string CounterClockwiseRotationSettings => _dict["CounterClockwiseRotationSettings"][Language];
        public static string CounterClockwiseRotationThreshold => _dict["CounterClockwiseRotationThreshold"][Language];
        public static string Display => _dict["Display"][Language];
        public static string Width => _dict["Width"][Language];
        public static string Height => _dict["Height"][Language];
        public static string Relative => _dict["Relative"][Language];
        public static string Rotation => _dict["Rotation"][Language];
        public static string ResetTime => _dict["ResetTime"][Language];
        public static string XSensitivity => _dict["XSensitivity"][Language];
        public static string YSensitivity => _dict["YSensitivity"][Language];
        public static string Lockaspectratio => _dict["Lockaspectratio"][Language];
        public static string Locktousablearea => _dict["Locktousablearea"][Language];
        public static string Clampinputoutsidearea => _dict["Clampinputoutsidearea"][Language];
        public static string Ignoreinputoutsidearea => _dict["Ignoreinputoutsidearea"][Language];
        public static string Handedness => _dict["Handedness"][Language];
        public static string Fullarea => _dict["Fullarea"][Language];
        public static string Quarterarea => _dict["Quarterarea"][Language];
        public static string Converter => _dict["Converter"][Language];
        public static string Convertarea => _dict["Convertarea"][Language];
        public static string Left => _dict["Left"][Language];
        public static string Right => _dict["Right"][Language];
        public static string Top => _dict["Top"][Language];
        public static string Bottom => _dict["Bottom"][Language];
        public static string Center => _dict["Center"][Language];
        public static string Horizontal => _dict["Horizontal"][Language];
        public static string Vertical => _dict["Vertical"][Language];
        public static string Flip => _dict["Flip"][Language];
        public static string Resize => _dict["Resize"][Language];
        public static string Align => _dict["Align"][Language];
        public static string Apply => _dict["Apply"][Language];
        public static string Save => _dict["Save"][Language];
        public static string Cancel => _dict["Cancel"][Language];
        public static string Close => _dict["Close"][Language];
        public static string Ok => _dict["Ok"][Language];
        public static string Clear => _dict["Clear"][Language];
        public static string Refresh => _dict["Refresh"][Language];
        public static string Previous => _dict["Previous"][Language];
        public static string Next => _dict["Next"][Language];
        public static string Exit => _dict["Exit"][Language];
        public static string Quit => _dict["Quit"][Language];
        public static string Applysettings => _dict["Applysettings"][Language];
        public static string Savesettings => _dict["Savesettings"][Language];
        public static string Savesettingsas => _dict["Savesettingsas"][Language];
        public static string Loadsettings => _dict["Loadsettings"][Language];
        public static string Saveaspreset => _dict["Saveaspreset"][Language];
        public static string Resettodefaults => _dict["Resettodefaults"][Language];
        public static string Refreshpresets => _dict["Refreshpresets"][Language];
        public static string ForceRefresh => _dict["ForceRefresh"][Language];
        public static string Detecttablet => _dict["Detecttablet"][Language];
        public static string CloseWindow => _dict["CloseWindow"][Language];
        public static string ShowWindow => _dict["ShowWindow"][Language];
        public static string ShowOpenTabletDriver => _dict["ShowOpenTabletDriver"][Language];
        public static string CopyAll => _dict["CopyAll"][Language];
        public static string Copy => _dict["Copy"][Language];
        public static string Time => _dict["Time"][Language];
        public static string Level => _dict["Level"][Language];
        public static string Group => _dict["Group"][Language];
        public static string Message => _dict["Message"][Language];
        public static string AboutOpenTabletDriver => _dict["AboutOpenTabletDriver"][Language];
        public static string ApplicationError => _dict["ApplicationError"][Language];
        public static string BindingEditor => _dict["BindingEditor"][Language];
        public static string AdvancedBindingEditor => _dict["AdvancedBindingEditor"][Language];
        public static string PluginManager => _dict["PluginManager"][Language];
        public static string OpenTabletDriverGuide => _dict["OpenTabletDriverGuide"][Language];
        public static string OpenTabletDriverUpdater => _dict["OpenTabletDriverUpdater"][Language];
        public static string DeviceStringReader => _dict["DeviceStringReader"][Language];
        public static string Checkforupdates => _dict["Checkforupdates"][Language];
        public static string Exportdiagnostics => _dict["Exportdiagnostics"][Language];
        public static string ExportdiagnosticstoClipboard => _dict["ExportdiagnosticstoClipboard"][Language];
        public static string Tabletdebugger => _dict["Tabletdebugger"][Language];
        public static string OpenPluginManager => _dict["OpenPluginManager"][Language];
        public static string OpenPluginManager_1 => _dict["OpenPluginManager_1"][Language];
        public static string Openpluginsdirectory => _dict["Openpluginsdirectory"][Language];
        public static string Openrecordingsdirectory => _dict["Openrecordingsdirectory"][Language];
        public static string Showguide => _dict["Showguide"][Language];
        public static string OpenWiki => _dict["OpenWiki"][Language];
        public static string OpenDirectory => _dict["OpenDirectory"][Language];
        public static string Installplugin => _dict["Installplugin"][Language];
        public static string Uninstall => _dict["Uninstall"][Language];
        public static string Showpluginwiki => _dict["Showpluginwiki"][Language];
        public static string Showsourcecode => _dict["Showsourcecode"][Language];
        public static string GotoRelease => _dict["GotoRelease"][Language];
        public static string About => _dict["About"][Language];
        public static string Device => _dict["Device"][Language];
        public static string DeviceString => _dict["DeviceString"][Language];
        public static string DriverVersion => _dict["DriverVersion"][Language];
        public static string MaxSupportedDriverVersion => _dict["MaxSupportedDriverVersion"][Language];
        public static string ReportRate => _dict["ReportRate"][Language];
        public static string ReportsRecorded => _dict["ReportsRecorded"][Language];
        public static string Nostatsobservedyet => _dict["Nostatsobservedyet"][Language];
        public static string AdditionalStatistics => _dict["AdditionalStatistics"][Language];
        public static string AdditionalStats => _dict["AdditionalStats"][Language];
        public static string DataRecording => _dict["DataRecording"][Language];
        public static string Recording => _dict["Recording"][Language];
        public static string RawTabletData => _dict["RawTabletData"][Language];
        public static string RawDataMode => _dict["RawDataMode"][Language];
        public static string TabletReport => _dict["TabletReport"][Language];
        public static string DebuggedReports => _dict["DebuggedReports"][Language];
        public static string DebuggedTablets => _dict["DebuggedTablets"][Language];
        public static string Demo => _dict["Demo"][Language];
        public static string DemoAreaEditor => _dict["DemoAreaEditor"][Language];
        public static string DemoBinding => _dict["DemoBinding"][Language];
        public static string Visualizer => _dict["Visualizer"][Language];
        public static string DumpAll => _dict["DumpAll"][Language];
        public static string SendRequest => _dict["SendRequest"][Language];
        public static string Requirereconnectonfail => _dict["Requirereconnectonfail"][Language];
        public static string Dumppotentiallydangerousstrings => _dict["Dumppotentiallydangerousstrings"][Language];
        public static string Notabletsaredetected => _dict["Notabletsaredetected"][Language];
        public static string Notabletsweredetectedorselected => _dict["Notabletsweredetectedorselected"][Language];
        public static string Nosupportedoutputmodeselected => _dict["Nosupportedoutputmodeselected"][Language];
        public static string Nopresetsloaded => _dict["Nopresetsloaded"][Language];
        public static string Noupdatesareavailable => _dict["Noupdatesareavailable"][Language];
        public static string Checkingforupdates => _dict["Checkingforupdates"][Language];
        public static string ConnectingtoOpenTabletDriverDaemon => _dict["ConnectingtoOpenTabletDriverDaemon"][Language];
        public static string AnapplicationerrorhasoccuredReportthistothedevelopers => _dict["AnapplicationerrorhasoccuredReportthistothedevelopers"][Language];
        public static string OpenTabletDriverGithubRepository => _dict["OpenTabletDriverGithubRepository"][Language];
        public static string SourceCodeRepository => _dict["SourceCodeRepository"][Language];
        public static string Opensourcecrossplatformtabletconfigurator => _dict["Opensourcecrossplatformtabletconfigurator"][Language];
        public static string License => _dict["License"][Language];
        public static string Credits => _dict["Credits"][Language];
        public static string Creator => _dict["Creator"][Language];
        public static string Owner => _dict["Owner"][Language];
        public static string Name => _dict["Name"][Language];
        public static string Type => _dict["Type"][Language];
        public static string Description => _dict["Description"][Language];
        public static string Wiki => _dict["Wiki"][Language];
        public static string Memoriam => _dict["Memoriam"][Language];
        public static string Inmemoryofjamesbt365 => _dict["Inmemoryofjamesbt365"][Language];
        public static string Usealternatesource => _dict["Usealternatesource"][Language];
        public static string Nopluginselected => _dict["Nopluginselected"][Language];
        public static string Nopluginscontainingthistypeareinstalled => _dict["Nopluginscontainingthistypeareinstalled"][Language];
        public static string PluginVersion => _dict["PluginVersion"][Language];
        public static string Draganddroppluginsheretoinstall => _dict["Draganddroppluginsheretoinstall"][Language];
        public static string Theminimumthresholdinorderfortheassignedbindingtoactivate => _dict["Theminimumthresholdinorderfortheassignedbindingtoactivate"][Language];
        public static string Theminimumthresholdindegreesinorderfortheassignedbindingtoactivate => _dict["Theminimumthresholdindegreesinorderfortheassignedbindingtoactivate"][Language];
        public static string Disablepressureifitisavailable => _dict["Disablepressureifitisavailable"][Language];
        public static string Disabletiltifitisavailable => _dict["Disabletiltifitisavailable"][Language];
        public static string PenBindingsrequirepressuretoactivate => _dict["PenBindingsrequirepressuretoactivate"][Language];
        public static string Angleofrotationaboutthecenterofthearea => _dict["Angleofrotationaboutthecenterofthearea"][Language];
        public static string Youcanrightclicktheareaeditortoenableaspectratiolockingadjustalignmentorresizethearea => _dict["Youcanrightclicktheareaeditortoenableaspectratiolockingadjustalignmentorresizethearea"][Language];
        public static string Youcanrightclicktheareaeditortosettheareatoadisplayadjustalignmentorresizethearea => _dict["Youcanrightclicktheareaeditortosettheareatoadisplayadjustalignmentorresizethearea"][Language];
        public static string Requestsallstringsinastringdumpeveniftheyareknowntolikelydamageordisruptusageofthetablet => _dict["Requestsallstringsinastringdumpeveniftheyareknowntolikelydamageordisruptusageofthetablet"][Language];
        public static string Pausesstringdumpwithapopupboxifanystringdumperrorsoccur => _dict["Pausesstringdumpwithapopupboxifanystringdumperrorsoccur"][Language];
        public static string WARNINGnArbitraryuseofthistoolmaycausedamageordisruptusageofyourtablet => _dict["WARNINGnArbitraryuseofthistoolmaycausedamageordisruptusageofyourtablet"][Language];
        public static string Nosupportedoutputmodeselected_1 => _dict["Nosupportedoutputmodeselected_1"][Language];
        public static string Notabletsaredetected_1 => _dict["Notabletsaredetected_1"][Language];
        public static string Notabletisselected => _dict["Notabletisselected"][Language];
        public static string Notabletdetected => _dict["Notabletdetected"][Language];
        public static string Nosupportedoutputmodeselected_2 => _dict["Nosupportedoutputmodeselected_2"][Language];
        public static string Memo => _dict["Memo"][Language];
        public static string File => _dict["File"][Language];
        public static string Tablets => _dict["Tablets"][Language];
        public static string Help => _dict["Help"][Language];
        public static string Plugins1 => _dict["Plugins1"][Language];
        public static string LanguageMenu => _dict["LanguageMenu"][Language];

        public static string[] SupportedLanguages => new[] {"en","zh","ko","es","ru","fr","de"};
        public static string GetLanguageName(string code) => code switch
        {
            "en" => "English",
            "zh" => "中文",
            "ko" => "한국어",
            "es" => "Español",
            "ru" => "Русский",
            "fr" => "Français",
            "de" => "Deutsch",
            _ => "English"
        };

        private static readonly Dictionary<string, Dictionary<string, string>> _dict = new()
        {
            ["Output"] = new() { ["en"] = "Output", ["zh"] = "输出", ["ko"] = "출력", ["es"] = "Salida", ["ru"] = "Вывод", ["fr"] = "Sortie", ["de"] = "Ausgabe" },
            ["Filters"] = new() { ["en"] = "Filters", ["zh"] = "滤镜", ["ko"] = "필터", ["es"] = "Filtros", ["ru"] = "Фильтры", ["fr"] = "Filtres", ["de"] = "Filter" },
            ["PenSettings"] = new() { ["en"] = "Pen Settings", ["zh"] = "笔设置", ["ko"] = "펜 설정", ["es"] = "Config. de lápiz", ["ru"] = "Настройки пера", ["fr"] = "Param. du stylet", ["de"] = "Stifteinstellungen" },
            ["AuxiliarySettings"] = new() { ["en"] = "Auxiliary Settings", ["zh"] = "辅助设置", ["ko"] = "보조 설정", ["es"] = "Config. auxiliar", ["ru"] = "Доп. настройки", ["fr"] = "Param. auxiliaires", ["de"] = "Hilfseinstellungen" },
            ["MouseSettings"] = new() { ["en"] = "Mouse Settings", ["zh"] = "鼠标设置", ["ko"] = "마우스 설정", ["es"] = "Config. de ratón", ["ru"] = "Настройки мыши", ["fr"] = "Param. de souris", ["de"] = "Mauseinstellungen" },
            ["Tools"] = new() { ["en"] = "Tools", ["zh"] = "工具", ["ko"] = "도구", ["es"] = "Herramientas", ["ru"] = "Инструменты", ["fr"] = "Outils", ["de"] = "Werkzeuge" },
            ["Info"] = new() { ["en"] = "Info", ["zh"] = "信息", ["ko"] = "정보", ["es"] = "Información", ["ru"] = "Информация", ["fr"] = "Informations", ["de"] = "Info" },
            ["Console"] = new() { ["en"] = "Console", ["zh"] = "控制台", ["ko"] = "콘솔", ["es"] = "Consola", ["ru"] = "Консоль", ["fr"] = "Console", ["de"] = "Konsole" },
            ["Tablet"] = new() { ["en"] = "Tablet", ["zh"] = "数位板", ["ko"] = "태블릿", ["es"] = "Tableta", ["ru"] = "Планшет", ["fr"] = "Tablette", ["de"] = "Tablet" },
            ["Plugins"] = new() { ["en"] = "Plugins", ["zh"] = "插件", ["ko"] = "플러그인", ["es"] = "Complementos", ["ru"] = "Плагины", ["fr"] = "Extensions", ["de"] = "Plugins" },
            ["Presets"] = new() { ["en"] = "Presets", ["zh"] = "预设", ["ko"] = "프리셋", ["es"] = "Preajustes", ["ru"] = "Предустановки", ["fr"] = "Préréglages", ["de"] = "Voreinstellungen" },
            ["TipSettings"] = new() { ["en"] = "Tip Settings", ["zh"] = "笔尖设置", ["ko"] = "펜촉 설정", ["es"] = "Config. de punta", ["ru"] = "Настройки наконечника", ["fr"] = "Param. de pointe", ["de"] = "Spitzeneinstellungen" },
            ["TipBinding"] = new() { ["en"] = "Tip Binding", ["zh"] = "笔尖绑定", ["ko"] = "펜촉 바인딩", ["es"] = "Vínculo de punta", ["ru"] = "Привязка наконечника", ["fr"] = "Liaison de pointe", ["de"] = "Spitzenbindung" },
            ["TipThreshold"] = new() { ["en"] = "Tip Threshold", ["zh"] = "笔尖阈值", ["ko"] = "펜촉 임계값", ["es"] = "Umbral de punta", ["ru"] = "Порог наконечника", ["fr"] = "Seuil de pointe", ["de"] = "Spitzenschwelle" },
            ["EraserSettings"] = new() { ["en"] = "Eraser Settings", ["zh"] = "橡皮擦设置", ["ko"] = "지우개 설정", ["es"] = "Config. de borrador", ["ru"] = "Настройки ластика", ["fr"] = "Param. de gomme", ["de"] = "Radierereinstellungen" },
            ["EraserBinding"] = new() { ["en"] = "Eraser Binding", ["zh"] = "橡皮擦绑定", ["ko"] = "지우개 바인딩", ["es"] = "Vínculo de borrador", ["ru"] = "Привязка ластика", ["fr"] = "Liaison de gomme", ["de"] = "Radiererbindung" },
            ["EraserThreshold"] = new() { ["en"] = "Eraser Threshold", ["zh"] = "橡皮擦阈值", ["ko"] = "지우개 임계값", ["es"] = "Umbral de borrador", ["ru"] = "Порог ластика", ["fr"] = "Seuil de gomme", ["de"] = "Radiererschwelle" },
            ["PenButtons"] = new() { ["en"] = "Pen Buttons", ["zh"] = "笔按键", ["ko"] = "펜 버튼", ["es"] = "Botones del lápiz", ["ru"] = "Кнопки пера", ["fr"] = "Boutons du stylet", ["de"] = "Stifttasten" },
            ["DisablePressure"] = new() { ["en"] = "Disable Pressure", ["zh"] = "禁用压感", ["ko"] = "필압 비활성화", ["es"] = "Desactivar presión", ["ru"] = "Откл. давление", ["fr"] = "Désactiver pression", ["de"] = "Druck deaktivieren" },
            ["DisableTilt"] = new() { ["en"] = "Disable Tilt", ["zh"] = "禁用倾斜", ["ko"] = "기울기 비활성화", ["es"] = "Desactivar inclinación", ["ru"] = "Откл. наклон", ["fr"] = "Désactiver inclinaison", ["de"] = "Neigung deaktivieren" },
            ["DragBindings"] = new() { ["en"] = "Drag Bindings", ["zh"] = "拖拽绑定", ["ko"] = "드래그 바인딩", ["es"] = "Vínculos de arrastre", ["ru"] = "Привязки перетаскивания", ["fr"] = "Liaisons de glissement", ["de"] = "Ziehbindungen" },
            ["Miscellaneous"] = new() { ["en"] = "Miscellaneous", ["zh"] = "杂项", ["ko"] = "기타", ["es"] = "Varios", ["ru"] = "Разное", ["fr"] = "Divers", ["de"] = "Sonstiges" },
            ["Auxiliary"] = new() { ["en"] = "Auxiliary", ["zh"] = "辅助按键", ["ko"] = "보조 키", ["es"] = "Auxiliar", ["ru"] = "Вспомогательные", ["fr"] = "Auxiliaire", ["de"] = "Hilfstasten" },
            ["WheelButtons"] = new() { ["en"] = "Wheel Buttons", ["zh"] = "滚轮按键", ["ko"] = "휠 버튼", ["es"] = "Botones de rueda", ["ru"] = "Кнопки колеса", ["fr"] = "Boutons de molette", ["de"] = "Radknöpfe" },
            ["MouseButtons"] = new() { ["en"] = "Mouse Buttons", ["zh"] = "鼠标按键", ["ko"] = "마우스 버튼", ["es"] = "Botones de ratón", ["ru"] = "Кнопки мыши", ["fr"] = "Boutons de souris", ["de"] = "Maustasten" },
            ["MouseScrollwheel"] = new() { ["en"] = "Mouse Scrollwheel", ["zh"] = "鼠标滚轮", ["ko"] = "마우스 스크롤휠", ["es"] = "Rueda de ratón", ["ru"] = "Колесо мыши", ["fr"] = "Molette de souris", ["de"] = "Mausrad" },
            ["ScrollUp"] = new() { ["en"] = "Scroll Up", ["zh"] = "上滚", ["ko"] = "위로 스크롤", ["es"] = "Arriba", ["ru"] = "Вверх", ["fr"] = "Défiler haut", ["de"] = "Hoch scrollen" },
            ["ScrollDown"] = new() { ["en"] = "Scroll Down", ["zh"] = "下滚", ["ko"] = "아래로 스크롤", ["es"] = "Abajo", ["ru"] = "Вниз", ["fr"] = "Défiler bas", ["de"] = "Runter scrollen" },
            ["ClockwiseRotation"] = new() { ["en"] = "Clockwise Rotation", ["zh"] = "顺时针旋转", ["ko"] = "시계 방향 회전", ["es"] = "Rotación horaria", ["ru"] = "По часовой", ["fr"] = "Rotation horaire", ["de"] = "Uhrzeigersinn" },
            ["ClockwiseRotationSettings"] = new() { ["en"] = "Clockwise Rotation Settings", ["zh"] = "顺时针旋转设置", ["ko"] = "시계 방향 회전 설정", ["es"] = "Config. rot. horaria", ["ru"] = "Настр. по часовой", ["fr"] = "Param. rot. horaire", ["de"] = "Einst. Uhrzeigersinn" },
            ["ClockwiseRotationThreshold"] = new() { ["en"] = "Clockwise Rotation Threshold", ["zh"] = "顺时针旋转阈值", ["ko"] = "시계 방향 회전 임계값", ["es"] = "Umbral rot. horaria", ["ru"] = "Порог по часовой", ["fr"] = "Seuil rot. horaire", ["de"] = "Schwelle Uhrzeigersinn" },
            ["CounterClockwiseRotation"] = new() { ["en"] = "Counter-Clockwise Rotation", ["zh"] = "逆时针旋转", ["ko"] = "반시계 방향 회전", ["es"] = "Rotación antihoraria", ["ru"] = "Против часовой", ["fr"] = "Rotation antihoraire", ["de"] = "Gegenuhrzeigersinn" },
            ["CounterClockwiseRotationSettings"] = new() { ["en"] = "Counter-Clockwise Rotation Settings", ["zh"] = "逆时针旋转设置", ["ko"] = "반시계 방향 회전 설정", ["es"] = "Config. rot. antihoraria", ["ru"] = "Настр. против часовой", ["fr"] = "Param. rot. antihoraire", ["de"] = "Einst. Gegenuhrzeigersinn" },
            ["CounterClockwiseRotationThreshold"] = new() { ["en"] = "Counter-Clockwise Rotation Threshold", ["zh"] = "逆时针旋转阈值", ["ko"] = "반시계 방향 회전 임계값", ["es"] = "Umbral rot. antihoraria", ["ru"] = "Порог против часовой", ["fr"] = "Seuil rot. antihoraire", ["de"] = "Schwelle Gegenuhrzeigersinn" },
            ["Display"] = new() { ["en"] = "Display", ["zh"] = "显示器", ["ko"] = "디스플레이", ["es"] = "Pantalla", ["ru"] = "Дисплей", ["fr"] = "Écran", ["de"] = "Anzeige" },
            ["Width"] = new() { ["en"] = "Width", ["zh"] = "宽度", ["ko"] = "너비", ["es"] = "Ancho", ["ru"] = "Ширина", ["fr"] = "Largeur", ["de"] = "Breite" },
            ["Height"] = new() { ["en"] = "Height", ["zh"] = "高度", ["ko"] = "높이", ["es"] = "Alto", ["ru"] = "Высота", ["fr"] = "Hauteur", ["de"] = "Höhe" },
            ["Relative"] = new() { ["en"] = "Relative", ["zh"] = "相对模式", ["ko"] = "상대 모드", ["es"] = "Relativo", ["ru"] = "Относительный", ["fr"] = "Relatif", ["de"] = "Relativ" },
            ["Rotation"] = new() { ["en"] = "Rotation", ["zh"] = "旋转", ["ko"] = "회전", ["es"] = "Rotación", ["ru"] = "Вращение", ["fr"] = "Rotation", ["de"] = "Drehung" },
            ["ResetTime"] = new() { ["en"] = "Reset Time", ["zh"] = "重置时间", ["ko"] = "리셋 시간", ["es"] = "Tiempo de reinicio", ["ru"] = "Время сброса", ["fr"] = "Temps de réinitialisation", ["de"] = "Rücksetzzeit" },
            ["XSensitivity"] = new() { ["en"] = "X Sensitivity", ["zh"] = "X 灵敏度", ["ko"] = "X 감도", ["es"] = "Sensibilidad X", ["ru"] = "Чувств. X", ["fr"] = "Sensibilité X", ["de"] = "X-Empfindlichkeit" },
            ["YSensitivity"] = new() { ["en"] = "Y Sensitivity", ["zh"] = "Y 灵敏度", ["ko"] = "Y 감도", ["es"] = "Sensibilidad Y", ["ru"] = "Чувств. Y", ["fr"] = "Sensibilité Y", ["de"] = "Y-Empfindlichkeit" },
            ["Lockaspectratio"] = new() { ["en"] = "Lock aspect ratio", ["zh"] = "锁定宽高比", ["ko"] = "종횡비 잠금", ["es"] = "Bloquear aspecto", ["ru"] = "Блок. пропорции", ["fr"] = "Verrouiller ratio", ["de"] = "Seitenverhältnis sperren" },
            ["Locktousablearea"] = new() { ["en"] = "Lock to usable area", ["zh"] = "锁定到可用区域", ["ko"] = "사용 영역에 잠금", ["es"] = "Bloquear a área útil", ["ru"] = "Блок. на раб. область", ["fr"] = "Verrouiller zone utile", ["de"] = "Auf Nutzbereich sperren" },
            ["Clampinputoutsidearea"] = new() { ["en"] = "Clamp input outside area", ["zh"] = "限制区域外输入", ["ko"] = "영역 밖 입력 제한", ["es"] = "Restringir entrada fuera", ["ru"] = "Ограничить ввод снаружи", ["fr"] = "Limiter entrée hors zone", ["de"] = "Eingabe außerhalb begrenzen" },
            ["Ignoreinputoutsidearea"] = new() { ["en"] = "Ignore input outside area", ["zh"] = "忽略区域外输入", ["ko"] = "영역 밖 입력 무시", ["es"] = "Ignorar entrada fuera", ["ru"] = "Игнор. ввод снаружи", ["fr"] = "Ignorer entrée hors zone", ["de"] = "Eingabe außerhalb ignorieren" },
            ["Handedness"] = new() { ["en"] = "Handedness", ["zh"] = "惯用手", ["ko"] = "손 방향", ["es"] = "Mano dominante", ["ru"] = "Ведущая рука", ["fr"] = "Main dominante", ["de"] = "Händigkeit" },
            ["Fullarea"] = new() { ["en"] = "Full area", ["zh"] = "全区域", ["ko"] = "전체 영역", ["es"] = "Área completa", ["ru"] = "Вся область", ["fr"] = "Zone complète", ["de"] = "Gesamter Bereich" },
            ["Quarterarea"] = new() { ["en"] = "Quarter area", ["zh"] = "四分之一区域", ["ko"] = "1/4 영역", ["es"] = "Cuarto de área", ["ru"] = "Четверть области", ["fr"] = "Quart de zone", ["de"] = "Viertelbereich" },
            ["Converter"] = new() { ["en"] = "Converter", ["zh"] = "转换器", ["ko"] = "변환기", ["es"] = "Convertidor", ["ru"] = "Конвертер", ["fr"] = "Convertisseur", ["de"] = "Konverter" },
            ["Convertarea"] = new() { ["en"] = "Convert area...", ["zh"] = "转换区域...", ["ko"] = "영역 변환...", ["es"] = "Convertir área...", ["ru"] = "Конвертировать область...", ["fr"] = "Convertir zone...", ["de"] = "Bereich konvertieren..." },
            ["Left"] = new() { ["en"] = "Left", ["zh"] = "左手", ["ko"] = "왼손", ["es"] = "Izquierda", ["ru"] = "Левая", ["fr"] = "Gauche", ["de"] = "Links" },
            ["Right"] = new() { ["en"] = "Right", ["zh"] = "右手", ["ko"] = "오른손", ["es"] = "Derecha", ["ru"] = "Правая", ["fr"] = "Droite", ["de"] = "Rechts" },
            ["Top"] = new() { ["en"] = "Top", ["zh"] = "顶部", ["ko"] = "위", ["es"] = "Arriba", ["ru"] = "Верх", ["fr"] = "Haut", ["de"] = "Oben" },
            ["Bottom"] = new() { ["en"] = "Bottom", ["zh"] = "底部", ["ko"] = "아래", ["es"] = "Abajo", ["ru"] = "Низ", ["fr"] = "Bas", ["de"] = "Unten" },
            ["Center"] = new() { ["en"] = "Center", ["zh"] = "居中", ["ko"] = "중앙", ["es"] = "Centro", ["ru"] = "Центр", ["fr"] = "Centre", ["de"] = "Mitte" },
            ["Horizontal"] = new() { ["en"] = "Horizontal", ["zh"] = "水平", ["ko"] = "수평", ["es"] = "Horizontal", ["ru"] = "Горизонтально", ["fr"] = "Horizontal", ["de"] = "Horizontal" },
            ["Vertical"] = new() { ["en"] = "Vertical", ["zh"] = "垂直", ["ko"] = "수직", ["es"] = "Vertical", ["ru"] = "Вертикально", ["fr"] = "Vertical", ["de"] = "Vertikal" },
            ["Flip"] = new() { ["en"] = "Flip", ["zh"] = "翻转", ["ko"] = "뒤집기", ["es"] = "Voltear", ["ru"] = "Отразить", ["fr"] = "Retourner", ["de"] = "Spiegeln" },
            ["Resize"] = new() { ["en"] = "Resize", ["zh"] = "调整大小", ["ko"] = "크기 조정", ["es"] = "Redimensionar", ["ru"] = "Изм. размер", ["fr"] = "Redimensionner", ["de"] = "Größe ändern" },
            ["Align"] = new() { ["en"] = "Align", ["zh"] = "对齐", ["ko"] = "정렬", ["es"] = "Alinear", ["ru"] = "Выровнять", ["fr"] = "Aligner", ["de"] = "Ausrichten" },
            ["Apply"] = new() { ["en"] = "Apply", ["zh"] = "应用", ["ko"] = "적용", ["es"] = "Aplicar", ["ru"] = "Применить", ["fr"] = "Appliquer", ["de"] = "Anwenden" },
            ["Save"] = new() { ["en"] = "Save", ["zh"] = "保存", ["ko"] = "저장", ["es"] = "Guardar", ["ru"] = "Сохранить", ["fr"] = "Enregistrer", ["de"] = "Speichern" },
            ["Cancel"] = new() { ["en"] = "Cancel", ["zh"] = "取消", ["ko"] = "취소", ["es"] = "Cancelar", ["ru"] = "Отмена", ["fr"] = "Annuler", ["de"] = "Abbrechen" },
            ["Close"] = new() { ["en"] = "Close", ["zh"] = "关闭", ["ko"] = "닫기", ["es"] = "Cerrar", ["ru"] = "Закрыть", ["fr"] = "Fermer", ["de"] = "Schließen" },
            ["Ok"] = new() { ["en"] = "Ok", ["zh"] = "确定", ["ko"] = "확인", ["es"] = "Aceptar", ["ru"] = "ОК", ["fr"] = "OK", ["de"] = "OK" },
            ["Clear"] = new() { ["en"] = "Clear", ["zh"] = "清除", ["ko"] = "지우기", ["es"] = "Limpiar", ["ru"] = "Очистить", ["fr"] = "Effacer", ["de"] = "Löschen" },
            ["Refresh"] = new() { ["en"] = "Refresh", ["zh"] = "刷新", ["ko"] = "새로고침", ["es"] = "Actualizar", ["ru"] = "Обновить", ["fr"] = "Actualiser", ["de"] = "Aktualisieren" },
            ["Previous"] = new() { ["en"] = "Previous", ["zh"] = "上一步", ["ko"] = "이전", ["es"] = "Anterior", ["ru"] = "Назад", ["fr"] = "Précédent", ["de"] = "Zurück" },
            ["Next"] = new() { ["en"] = "Next", ["zh"] = "下一步", ["ko"] = "다음", ["es"] = "Siguiente", ["ru"] = "Далее", ["fr"] = "Suivant", ["de"] = "Weiter" },
            ["Exit"] = new() { ["en"] = "Exit", ["zh"] = "退出", ["ko"] = "종료", ["es"] = "Salir", ["ru"] = "Выход", ["fr"] = "Quitter", ["de"] = "Beenden" },
            ["Quit"] = new() { ["en"] = "Quit", ["zh"] = "退出", ["ko"] = "종료", ["es"] = "Salir", ["ru"] = "Выход", ["fr"] = "Quitter", ["de"] = "Beenden" },
            ["Applysettings"] = new() { ["en"] = "Apply settings", ["zh"] = "应用设置", ["ko"] = "설정 적용", ["es"] = "Aplicar config.", ["ru"] = "Применить настройки", ["fr"] = "Appliquer param.", ["de"] = "Einst. anwenden" },
            ["Savesettings"] = new() { ["en"] = "Save settings", ["zh"] = "保存设置", ["ko"] = "설정 저장", ["es"] = "Guardar config.", ["ru"] = "Сохранить настройки", ["fr"] = "Enregistrer param.", ["de"] = "Einst. speichern" },
            ["Savesettingsas"] = new() { ["en"] = "Save settings as...", ["zh"] = "另存设置...", ["ko"] = "다른 이름으로 저장...", ["es"] = "Guardar config. como...", ["ru"] = "Сохранить как...", ["fr"] = "Enregistrer sous...", ["de"] = "Einst. speichern als..." },
            ["Loadsettings"] = new() { ["en"] = "Load settings...", ["zh"] = "加载设置...", ["ko"] = "설정 불러오기...", ["es"] = "Cargar config.", ["ru"] = "Загрузить настройки...", ["fr"] = "Charger param.", ["de"] = "Einst. laden..." },
            ["Saveaspreset"] = new() { ["en"] = "Save as preset...", ["zh"] = "保存为预设...", ["ko"] = "프리셋으로 저장...", ["es"] = "Guardar como preajuste...", ["ru"] = "Сохранить как предустановку...", ["fr"] = "Enregistrer comme préréglage...", ["de"] = "Als Voreinstellung speichern..." },
            ["Resettodefaults"] = new() { ["en"] = "Reset to defaults", ["zh"] = "恢复默认", ["ko"] = "기본값으로 초기화", ["es"] = "Restablecer predet.", ["ru"] = "Сбросить по умолчанию", ["fr"] = "Réinitialiser par défaut", ["de"] = "Auf Standard zurücksetzen" },
            ["Refreshpresets"] = new() { ["en"] = "Refresh presets", ["zh"] = "刷新预设", ["ko"] = "프리셋 새로고침", ["es"] = "Actualizar preajustes", ["ru"] = "Обновить предустановки", ["fr"] = "Actualiser préréglages", ["de"] = "Voreinst. aktualisieren" },
            ["ForceRefresh"] = new() { ["en"] = "Force Refresh", ["zh"] = "强制刷新", ["ko"] = "강제 새로고침", ["es"] = "Forzar actualización", ["ru"] = "Принуд. обновление", ["fr"] = "Forcer actualisation", ["de"] = "Aktualisierung erzwingen" },
            ["Detecttablet"] = new() { ["en"] = "Detect tablet", ["zh"] = "检测数位板", ["ko"] = "태블릿 감지", ["es"] = "Detectar tableta", ["ru"] = "Обнаружить планшет", ["fr"] = "Détecter tablette", ["de"] = "Tablet erkennen" },
            ["CloseWindow"] = new() { ["en"] = "Close Window", ["zh"] = "关闭窗口", ["ko"] = "창 닫기", ["es"] = "Cerrar ventana", ["ru"] = "Закрыть окно", ["fr"] = "Fermer fenêtre", ["de"] = "Fenster schließen" },
            ["ShowWindow"] = new() { ["en"] = "Show Window", ["zh"] = "显示窗口", ["ko"] = "창 표시", ["es"] = "Mostrar ventana", ["ru"] = "Показать окно", ["fr"] = "Afficher fenêtre", ["de"] = "Fenster anzeigen" },
            ["ShowOpenTabletDriver"] = new() { ["en"] = "Show OpenTabletDriver", ["zh"] = "显示 OpenTabletDriver", ["ko"] = "OpenTabletDriver 표시", ["es"] = "Mostrar OpenTabletDriver", ["ru"] = "Показать OpenTabletDriver", ["fr"] = "Afficher OpenTabletDriver", ["de"] = "OpenTabletDriver anzeigen" },
            ["CopyAll"] = new() { ["en"] = "Copy All", ["zh"] = "全部复制", ["ko"] = "전체 복사", ["es"] = "Copiar todo", ["ru"] = "Копировать всё", ["fr"] = "Tout copier", ["de"] = "Alles kopieren" },
            ["Copy"] = new() { ["en"] = "Copy", ["zh"] = "复制", ["ko"] = "복사", ["es"] = "Copiar", ["ru"] = "Копировать", ["fr"] = "Copier", ["de"] = "Kopieren" },
            ["Time"] = new() { ["en"] = "Time", ["zh"] = "时间", ["ko"] = "시간", ["es"] = "Hora", ["ru"] = "Время", ["fr"] = "Heure", ["de"] = "Zeit" },
            ["Level"] = new() { ["en"] = "Level", ["zh"] = "级别", ["ko"] = "레벨", ["es"] = "Nivel", ["ru"] = "Уровень", ["fr"] = "Niveau", ["de"] = "Stufe" },
            ["Group"] = new() { ["en"] = "Group", ["zh"] = "分组", ["ko"] = "그룹", ["es"] = "Grupo", ["ru"] = "Группа", ["fr"] = "Groupe", ["de"] = "Gruppe" },
            ["Message"] = new() { ["en"] = "Message", ["zh"] = "消息", ["ko"] = "메시지", ["es"] = "Mensaje", ["ru"] = "Сообщение", ["fr"] = "Message", ["de"] = "Nachricht" },
            ["AboutOpenTabletDriver"] = new() { ["en"] = "About OpenTabletDriver", ["zh"] = "关于 OpenTabletDriver", ["ko"] = "OpenTabletDriver 정보", ["es"] = "Acerca de OpenTabletDriver", ["ru"] = "О OpenTabletDriver", ["fr"] = "À propos d'OpenTabletDriver", ["de"] = "Über OpenTabletDriver" },
            ["ApplicationError"] = new() { ["en"] = "Application Error", ["zh"] = "程序错误", ["ko"] = "응용 프로그램 오류", ["es"] = "Error de aplicación", ["ru"] = "Ошибка приложения", ["fr"] = "Erreur d'application", ["de"] = "Anwendungsfehler" },
            ["BindingEditor"] = new() { ["en"] = "Binding Editor", ["zh"] = "按键编辑器", ["ko"] = "바인딩 편집기", ["es"] = "Editor de vínculos", ["ru"] = "Редактор привязок", ["fr"] = "Éditeur de liaisons", ["de"] = "Bindungseditor" },
            ["AdvancedBindingEditor"] = new() { ["en"] = "Advanced Binding Editor", ["zh"] = "高级按键编辑器", ["ko"] = "고급 바인딩 편집기", ["es"] = "Editor avanzado de vínculos", ["ru"] = "Расшир. редактор привязок", ["fr"] = "Éditeur avancé de liaisons", ["de"] = "Erweiterter Bindungseditor" },
            ["PluginManager"] = new() { ["en"] = "Plugin Manager", ["zh"] = "插件管理器", ["ko"] = "플러그인 관리자", ["es"] = "Gestor de complementos", ["ru"] = "Менеджер плагинов", ["fr"] = "Gestionnaire d'extensions", ["de"] = "Plugin-Manager" },
            ["OpenTabletDriverGuide"] = new() { ["en"] = "OpenTabletDriver Guide", ["zh"] = "OpenTabletDriver 指南", ["ko"] = "OpenTabletDriver 가이드", ["es"] = "Guía de OpenTabletDriver", ["ru"] = "Руководство OpenTabletDriver", ["fr"] = "Guide OpenTabletDriver", ["de"] = "OpenTabletDriver-Anleitung" },
            ["OpenTabletDriverUpdater"] = new() { ["en"] = "OpenTabletDriver Updater", ["zh"] = "OpenTabletDriver 更新器", ["ko"] = "OpenTabletDriver 업데이터", ["es"] = "Actualizador de OpenTabletDriver", ["ru"] = "Обновление OpenTabletDriver", ["fr"] = "Mise à jour OpenTabletDriver", ["de"] = "OpenTabletDriver-Updater" },
            ["DeviceStringReader"] = new() { ["en"] = "Device String Reader", ["zh"] = "设备字符串读取器", ["ko"] = "장치 문자열 리더", ["es"] = "Lector de cadena de dispositivo", ["ru"] = "Чтение строки устройства", ["fr"] = "Lecteur de chaîne de périph.", ["de"] = "Gerätestring-Leser" },
            ["Checkforupdates"] = new() { ["en"] = "Check for updates...", ["zh"] = "检查更新...", ["ko"] = "업데이트 확인...", ["es"] = "Buscar actualizaciones...", ["ru"] = "Проверить обновления...", ["fr"] = "Vérifier mises à jour...", ["de"] = "Nach Updates suchen..." },
            ["Exportdiagnostics"] = new() { ["en"] = "Export diagnostics...", ["zh"] = "导出诊断...", ["ko"] = "진단 내보내기...", ["es"] = "Exportar diagnóstico...", ["ru"] = "Экспорт диагностики...", ["fr"] = "Exporter diagnostic...", ["de"] = "Diagnose exportieren..." },
            ["ExportdiagnosticstoClipboard"] = new() { ["en"] = "Export diagnostics to Clipboard...", ["zh"] = "导出诊断到剪贴板...", ["ko"] = "진단을 클립보드로 내보내기...", ["es"] = "Exportar diag. al portapapeles...", ["ru"] = "Экспорт диагн. в буфер...", ["fr"] = "Exporter diag. vers presse-papiers...", ["de"] = "Diagnose in Zwischenablage..." },
            ["Tabletdebugger"] = new() { ["en"] = "Tablet debugger...", ["zh"] = "数位板调试器...", ["ko"] = "태블릿 디버거...", ["es"] = "Depurador de tableta...", ["ru"] = "Отладчик планшета...", ["fr"] = "Débogueur de tablette...", ["de"] = "Tablet-Debugger..." },
            ["OpenPluginManager"] = new() { ["en"] = "Open Plugin Manager", ["zh"] = "打开插件管理器", ["ko"] = "플러그인 관리자 열기", ["es"] = "Abrir gestor de complementos", ["ru"] = "Открыть менеджер плагинов", ["fr"] = "Ouvrir gestionnaire d'extensions", ["de"] = "Plugin-Manager öffnen" },
            ["OpenPluginManager_1"] = new() { ["en"] = "Open Plugin Manager...", ["zh"] = "打开插件管理器...", ["ko"] = "플러그인 관리자 열기...", ["es"] = "Abrir gestor de complementos...", ["ru"] = "Открыть менеджер плагинов...", ["fr"] = "Ouvrir gestionnaire d'extensions...", ["de"] = "Plugin-Manager öffnen..." },
            ["Openpluginsdirectory"] = new() { ["en"] = "Open plugins directory...", ["zh"] = "打开插件目录...", ["ko"] = "플러그인 디렉토리 열기...", ["es"] = "Abrir directorio de complementos...", ["ru"] = "Открыть папку плагинов...", ["fr"] = "Ouvrir dossier des extensions...", ["de"] = "Plugin-Verzeichnis öffnen..." },
            ["Openrecordingsdirectory"] = new() { ["en"] = "Open recordings directory...", ["zh"] = "打开录制目录...", ["ko"] = "녹화 디렉토리 열기...", ["es"] = "Abrir directorio de grabaciones...", ["ru"] = "Открыть папку записей...", ["fr"] = "Ouvrir dossier d'enregistrements...", ["de"] = "Aufnahmeverzeichnis öffnen..." },
            ["Showguide"] = new() { ["en"] = "Show guide...", ["zh"] = "显示指南...", ["ko"] = "가이드 보기...", ["es"] = "Mostrar guía...", ["ru"] = "Показать руководство...", ["fr"] = "Afficher le guide...", ["de"] = "Anleitung anzeigen..." },
            ["OpenWiki"] = new() { ["en"] = "Open Wiki...", ["zh"] = "打开 Wiki...", ["ko"] = "Wiki 열기...", ["es"] = "Abrir Wiki...", ["ru"] = "Открыть Wiki...", ["fr"] = "Ouvrir le Wiki...", ["de"] = "Wiki öffnen..." },
            ["OpenDirectory"] = new() { ["en"] = "Open Directory", ["zh"] = "打开目录", ["ko"] = "디렉토리 열기", ["es"] = "Abrir directorio", ["ru"] = "Открыть папку", ["fr"] = "Ouvrir le dossier", ["de"] = "Verzeichnis öffnen" },
            ["Installplugin"] = new() { ["en"] = "Install plugin...", ["zh"] = "安装插件...", ["ko"] = "플러그인 설치...", ["es"] = "Instalar complemento...", ["ru"] = "Установить плагин...", ["fr"] = "Installer extension...", ["de"] = "Plugin installieren..." },
            ["Uninstall"] = new() { ["en"] = "Uninstall", ["zh"] = "卸载", ["ko"] = "제거", ["es"] = "Desinstalar", ["ru"] = "Удалить", ["fr"] = "Désinstaller", ["de"] = "Deinstallieren" },
            ["Showpluginwiki"] = new() { ["en"] = "Show plugin wiki", ["zh"] = "显示插件 Wiki", ["ko"] = "플러그인 Wiki 표시", ["es"] = "Mostrar wiki de complemento", ["ru"] = "Показать вики плагина", ["fr"] = "Afficher wiki extension", ["de"] = "Plugin-Wiki anzeigen" },
            ["Showsourcecode"] = new() { ["en"] = "Show source code", ["zh"] = "显示源代码", ["ko"] = "소스 코드 표시", ["es"] = "Mostrar código fuente", ["ru"] = "Показать исходный код", ["fr"] = "Afficher code source", ["de"] = "Quellcode anzeigen" },
            ["GotoRelease"] = new() { ["en"] = "Go to Release", ["zh"] = "前往发布页", ["ko"] = "릴리스로 이동", ["es"] = "Ir a versión", ["ru"] = "Перейти к релизу", ["fr"] = "Aller à la version", ["de"] = "Zum Release" },
            ["About"] = new() { ["en"] = "About...", ["zh"] = "关于...", ["ko"] = "정보...", ["es"] = "Acerca de...", ["ru"] = "О программе...", ["fr"] = "À propos...", ["de"] = "Über..." },
            ["Device"] = new() { ["en"] = "Device", ["zh"] = "设备", ["ko"] = "장치", ["es"] = "Dispositivo", ["ru"] = "Устройство", ["fr"] = "Périphérique", ["de"] = "Gerät" },
            ["DeviceString"] = new() { ["en"] = "Device String", ["zh"] = "设备字符串", ["ko"] = "장치 문자열", ["es"] = "Cadena de dispositivo", ["ru"] = "Строка устройства", ["fr"] = "Chaîne de périphérique", ["de"] = "Gerätestring" },
            ["DriverVersion"] = new() { ["en"] = "Driver Version", ["zh"] = "驱动版本", ["ko"] = "드라이버 버전", ["es"] = "Versión del controlador", ["ru"] = "Версия драйвера", ["fr"] = "Version du pilote", ["de"] = "Treiberversion" },
            ["MaxSupportedDriverVersion"] = new() { ["en"] = "Max Supported Driver Version", ["zh"] = "最高支持驱动版本", ["ko"] = "최대 지원 드라이버 버전", ["es"] = "Versión máx. de controlador", ["ru"] = "Макс. версия драйвера", ["fr"] = "Version max. du pilote", ["de"] = "Max. Treiberversion" },
            ["ReportRate"] = new() { ["en"] = "Report Rate", ["zh"] = "回报率", ["ko"] = "보고율", ["es"] = "Tasa de informe", ["ru"] = "Частота отчётов", ["fr"] = "Taux de rapport", ["de"] = "Berichtsrate" },
            ["ReportsRecorded"] = new() { ["en"] = "Reports Recorded", ["zh"] = "已录制报告数", ["ko"] = "기록된 보고서", ["es"] = "Informes grabados", ["ru"] = "Записанные отчёты", ["fr"] = "Rapports enregistrés", ["de"] = "Aufgezeichnete Berichte" },
            ["Nostatsobservedyet"] = new() { ["en"] = "No stats observed yet", ["zh"] = "暂无统计数据", ["ko"] = "아직 통계 없음", ["es"] = "Sin estadísticas aún", ["ru"] = "Статистики пока нет", ["fr"] = "Pas encore de statistiques", ["de"] = "Noch keine Statistiken" },
            ["AdditionalStatistics"] = new() { ["en"] = "Additional Statistics", ["zh"] = "附加统计", ["ko"] = "추가 통계", ["es"] = "Estadísticas adicionales", ["ru"] = "Доп. статистика", ["fr"] = "Statistiques supplémentaires", ["de"] = "Zusätzliche Statistiken" },
            ["AdditionalStats"] = new() { ["en"] = "Additional Stats", ["zh"] = "附加统计", ["ko"] = "추가 통계", ["es"] = "Estadísticas adicionales", ["ru"] = "Доп. статистика", ["fr"] = "Stats supplémentaires", ["de"] = "Zusätzliche Stats" },
            ["DataRecording"] = new() { ["en"] = "Data Recording", ["zh"] = "数据录制", ["ko"] = "데이터 기록", ["es"] = "Grabación de datos", ["ru"] = "Запись данных", ["fr"] = "Enregistrement de données", ["de"] = "Datenaufzeichnung" },
            ["Recording"] = new() { ["en"] = "Recording", ["zh"] = "录制", ["ko"] = "기록 중", ["es"] = "Grabando", ["ru"] = "Запись", ["fr"] = "Enregistrement", ["de"] = "Aufnahme" },
            ["RawTabletData"] = new() { ["en"] = "Raw Tablet Data", ["zh"] = "原始数位板数据", ["ko"] = "원시 태블릿 데이터", ["es"] = "Datos brutos de tableta", ["ru"] = "Сырые данные планшета", ["fr"] = "Données brutes tablette", ["de"] = "Rohe Tabletdaten" },
            ["RawDataMode"] = new() { ["en"] = "Raw Data Mode", ["zh"] = "原始数据模式", ["ko"] = "원시 데이터 모드", ["es"] = "Modo de datos brutos", ["ru"] = "Режим сырых данных", ["fr"] = "Mode données brutes", ["de"] = "Rohdatenmodus" },
            ["TabletReport"] = new() { ["en"] = "Tablet Report", ["zh"] = "数位板报告", ["ko"] = "태블릿 보고서", ["es"] = "Informe de tableta", ["ru"] = "Отчёт планшета", ["fr"] = "Rapport de tablette", ["de"] = "Tablet-Bericht" },
            ["DebuggedReports"] = new() { ["en"] = "Debugged Reports", ["zh"] = "已调试报告", ["ko"] = "디버그된 보고서", ["es"] = "Informes depurados", ["ru"] = "Отлаженные отчёты", ["fr"] = "Rapports débogués", ["de"] = "Debug-Berichte" },
            ["DebuggedTablets"] = new() { ["en"] = "Debugged Tablets", ["zh"] = "已调式数位板", ["ko"] = "디버그된 태블릿", ["es"] = "Tabletas depuradas", ["ru"] = "Отлаженные планшеты", ["fr"] = "Tablettes déboguées", ["de"] = "Debug-Tablets" },
            ["Demo"] = new() { ["en"] = "Demo", ["zh"] = "演示", ["ko"] = "데모", ["es"] = "Demostración", ["ru"] = "Демо", ["fr"] = "Démo", ["de"] = "Demo" },
            ["DemoAreaEditor"] = new() { ["en"] = "Demo Area Editor", ["zh"] = "演示区域编辑器", ["ko"] = "데모 영역 편집기", ["es"] = "Editor de área de demo", ["ru"] = "Редактор области (демо)", ["fr"] = "Éditeur de zone (démo)", ["de"] = "Demo-Bereichseditor" },
            ["DemoBinding"] = new() { ["en"] = "Demo Binding", ["zh"] = "演示绑定", ["ko"] = "데모 바인딩", ["es"] = "Vínculo de demostración", ["ru"] = "Демо-привязка", ["fr"] = "Liaison de démo", ["de"] = "Demo-Bindung" },
            ["Visualizer"] = new() { ["en"] = "Visualizer", ["zh"] = "可视化", ["ko"] = "시각화 도구", ["es"] = "Visualizador", ["ru"] = "Визуализатор", ["fr"] = "Visualiseur", ["de"] = "Visualisierer" },
            ["DumpAll"] = new() { ["en"] = "Dump All", ["zh"] = "全部导出", ["ko"] = "전체 덤프", ["es"] = "Volcar todo", ["ru"] = "Дамп всего", ["fr"] = "Tout vider", ["de"] = "Alles ausgeben" },
            ["SendRequest"] = new() { ["en"] = "Send Request", ["zh"] = "发送请求", ["ko"] = "요청 보내기", ["es"] = "Enviar solicitud", ["ru"] = "Отправить запрос", ["fr"] = "Envoyer la requête", ["de"] = "Anfrage senden" },
            ["Requirereconnectonfail"] = new() { ["en"] = "Require reconnect on fail", ["zh"] = "失败时要求重连", ["ko"] = "실패 시 재연결 요구", ["es"] = "Reconectar al fallar", ["ru"] = "Переподключение при сбое", ["fr"] = "Reconnexion si échec", ["de"] = "Neuverbindung bei Fehler" },
            ["Dumppotentiallydangerousstrings"] = new() { ["en"] = "Dump potentially dangerous strings", ["zh"] = "导出可能有风险的字符串", ["ko"] = "위험 가능한 문자열 덤프", ["es"] = "Volcar cadenas potencialmente peligrosas", ["ru"] = "Дамп потенциально опасных строк", ["fr"] = "Vider chaînes potentiellement dangereuses", ["de"] = "Potenziell gefährliche Strings ausgeben" },
            ["Notabletsaredetected"] = new() { ["en"] = "No tablets are detected.", ["zh"] = "未检测到数位板。", ["ko"] = "태블릿이 감지되지 않았습니다.", ["es"] = "No se detectan tabletas.", ["ru"] = "Планшеты не обнаружены.", ["fr"] = "Aucune tablette détectée.", ["de"] = "Keine Tablets erkannt." },
            ["Notabletsweredetectedorselected"] = new() { ["en"] = "No tablets were detected or selected.", ["zh"] = "未检测到或选择数位板。", ["ko"] = "태블릿이 감지되거나 선택되지 않았습니다.", ["es"] = "No se detectaron o seleccionaron tabletas.", ["ru"] = "Планшеты не обнаружены или не выбраны.", ["fr"] = "Aucune tablette détectée ou sélectionnée.", ["de"] = "Keine Tablets erkannt oder ausgewählt." },
            ["Nosupportedoutputmodeselected"] = new() { ["en"] = "No supported output mode selected.", ["zh"] = "未选择支持的输出模式。", ["ko"] = "지원되는 출력 모드가 선택되지 않았습니다.", ["es"] = "No se seleccionó un modo de salida compatible.", ["ru"] = "Не выбран поддерживаемый режим вывода.", ["fr"] = "Aucun mode de sortie compatible sélectionné.", ["de"] = "Kein unterstützter Ausgabemodus ausgewählt." },
            ["Nopresetsloaded"] = new() { ["en"] = "No presets loaded", ["zh"] = "未加载预设", ["ko"] = "프리셋이 로드되지 않음", ["es"] = "Sin preajustes cargados", ["ru"] = "Предустановки не загружены", ["fr"] = "Aucun préréglage chargé", ["de"] = "Keine Voreinstellungen geladen" },
            ["Noupdatesareavailable"] = new() { ["en"] = "No updates are available.", ["zh"] = "暂无可用更新。", ["ko"] = "사용 가능한 업데이트가 없습니다.", ["es"] = "No hay actualizaciones disponibles.", ["ru"] = "Нет доступных обновлений.", ["fr"] = "Aucune mise à jour disponible.", ["de"] = "Keine Updates verfügbar." },
            ["Checkingforupdates"] = new() { ["en"] = "Checking for updates...", ["zh"] = "正在检查更新...", ["ko"] = "업데이트 확인 중...", ["es"] = "Buscando actualizaciones...", ["ru"] = "Проверка обновлений...", ["fr"] = "Vérification des mises à jour...", ["de"] = "Suche nach Updates..." },
            ["ConnectingtoOpenTabletDriverDaemon"] = new() { ["en"] = "Connecting to OpenTabletDriver Daemon...", ["zh"] = "正在连接 OpenTabletDriver 守护进程...", ["ko"] = "OpenTabletDriver 데몬에 연결 중...", ["es"] = "Conectando al demonio de OpenTabletDriver...", ["ru"] = "Подключение к демону OpenTabletDriver...", ["fr"] = "Connexion au démon OpenTabletDriver...", ["de"] = "Verbinde mit OpenTabletDriver-Daemon..." },
            ["AnapplicationerrorhasoccuredReportthistothedevelopers"] = new() { ["en"] = "An application error has occured. Report this to the developers!", ["zh"] = "程序发生错误。请向开发者反馈！", ["ko"] = "응용 프로그램 오류가 발생했습니다. 개발자에게 보고하세요!", ["es"] = "Ocurrió un error en la aplicación. ¡Reporte a los desarrolladores!", ["ru"] = "Произошла ошибка приложения. Сообщите разработчикам!", ["fr"] = "Une erreur est survenue. Signalez-la aux développeurs !", ["de"] = "Ein Anwendungsfehler ist aufgetreten. Bitte den Entwicklern melden!" },
            ["OpenTabletDriverGithubRepository"] = new() { ["en"] = "OpenTabletDriver Github Repository", ["zh"] = "OpenTabletDriver Github 仓库", ["ko"] = "OpenTabletDriver Github 저장소", ["es"] = "Repositorio Github de OpenTabletDriver", ["ru"] = "OpenTabletDriver на GitHub", ["fr"] = "Dépôt Github OpenTabletDriver", ["de"] = "OpenTabletDriver Github-Repository" },
            ["SourceCodeRepository"] = new() { ["en"] = "Source Code Repository", ["zh"] = "源代码仓库", ["ko"] = "소스 코드 저장소", ["es"] = "Repositorio de código fuente", ["ru"] = "Репозиторий исходного кода", ["fr"] = "Dépôt du code source", ["de"] = "Quellcode-Repository" },
            ["Opensourcecrossplatformtabletconfigurator"] = new() { ["en"] = "Open source, cross-platform tablet configurator", ["zh"] = "开源跨平台数位板配置工具", ["ko"] = "오픈 소스 크로스 플랫폼 태블릿 구성 도구", ["es"] = "Configurador de tabletas multiplataforma de código abierto", ["ru"] = "Кроссплатформенный конфигуратор планшетов с открытым исходным кодом", ["fr"] = "Configurateur de tablette open source multiplateforme", ["de"] = "Open-Source, plattformübergreifender Tablet-Konfigurator" },
            ["License"] = new() { ["en"] = "License", ["zh"] = "许可证", ["ko"] = "라이선스", ["es"] = "Licencia", ["ru"] = "Лицензия", ["fr"] = "Licence", ["de"] = "Lizenz" },
            ["Credits"] = new() { ["en"] = "Credits", ["zh"] = "鸣谢", ["ko"] = "크레딧", ["es"] = "Créditos", ["ru"] = "Благодарности", ["fr"] = "Crédits", ["de"] = "Mitwirkende" },
            ["Creator"] = new() { ["en"] = "Creator", ["zh"] = "创建者", ["ko"] = "제작자", ["es"] = "Creador", ["ru"] = "Создатель", ["fr"] = "Créateur", ["de"] = "Ersteller" },
            ["Owner"] = new() { ["en"] = "Owner", ["zh"] = "所有者", ["ko"] = "소유자", ["es"] = "Propietario", ["ru"] = "Владелец", ["fr"] = "Propriétaire", ["de"] = "Besitzer" },
            ["Name"] = new() { ["en"] = "Name", ["zh"] = "名称", ["ko"] = "이름", ["es"] = "Nombre", ["ru"] = "Имя", ["fr"] = "Nom", ["de"] = "Name" },
            ["Type"] = new() { ["en"] = "Type", ["zh"] = "类型", ["ko"] = "유형", ["es"] = "Tipo", ["ru"] = "Тип", ["fr"] = "Type", ["de"] = "Typ" },
            ["Description"] = new() { ["en"] = "Description", ["zh"] = "描述", ["ko"] = "설명", ["es"] = "Descripción", ["ru"] = "Описание", ["fr"] = "Description", ["de"] = "Beschreibung" },
            ["Wiki"] = new() { ["en"] = "Wiki", ["zh"] = "Wiki", ["ko"] = "Wiki", ["es"] = "Wiki", ["ru"] = "Вики", ["fr"] = "Wiki", ["de"] = "Wiki" },
            ["Memoriam"] = new() { ["en"] = "Memoriam", ["zh"] = "悼念", ["ko"] = "추모", ["es"] = "Conmemoración", ["ru"] = "Память", ["fr"] = "Mémoriam", ["de"] = "Gedenken" },
            ["Inmemoryofjamesbt365"] = new() { ["en"] = "In memory of jamesbt365", ["zh"] = "谨此纪念 jamesbt365", ["ko"] = "jamesbt365님을 기리며", ["es"] = "En memoria de jamesbt365", ["ru"] = "В память о jamesbt365", ["fr"] = "À la mémoire de jamesbt365", ["de"] = "In Gedenken an jamesbt365" },
            ["Usealternatesource"] = new() { ["en"] = "Use alternate source...", ["zh"] = "使用备用源...", ["ko"] = "대체 소스 사용...", ["es"] = "Usar fuente alternativa...", ["ru"] = "Использовать другой источник...", ["fr"] = "Utiliser source alternative...", ["de"] = "Alternative Quelle verwenden..." },
            ["Nopluginselected"] = new() { ["en"] = "No plugin selected.", ["zh"] = "未选择插件。", ["ko"] = "선택된 플러그인이 없습니다.", ["es"] = "Ningún complemento seleccionado.", ["ru"] = "Плагин не выбран.", ["fr"] = "Aucune extension sélectionnée.", ["de"] = "Kein Plugin ausgewählt." },
            ["Nopluginscontainingthistypeareinstalled"] = new() { ["en"] = "No plugins containing this type are installed.", ["zh"] = "未安装包含此类型的插件。", ["ko"] = "이 유형을 포함하는 플러그인이 설치되지 않았습니다.", ["es"] = "No hay complementos de este tipo instalados.", ["ru"] = "Плагины этого типа не установлены.", ["fr"] = "Aucune extension de ce type installée.", ["de"] = "Keine Plugins dieses Typs installiert." },
            ["PluginVersion"] = new() { ["en"] = "Plugin Version", ["zh"] = "插件版本", ["ko"] = "플러그인 버전", ["es"] = "Versión del complemento", ["ru"] = "Версия плагина", ["fr"] = "Version de l'extension", ["de"] = "Plugin-Version" },
            ["Draganddroppluginsheretoinstall"] = new() { ["en"] = "Drag and drop plugins here to install.", ["zh"] = "拖放插件到此处安装。", ["ko"] = "플러그인을 여기에 드래그하여 설치하세요.", ["es"] = "Arrastre complementos aquí para instalar.", ["ru"] = "Перетащите плагины сюда для установки.", ["fr"] = "Glissez-déposez les extensions ici.", ["de"] = "Plugins hierher ziehen zum Installieren." },
            ["Theminimumthresholdinorderfortheassignedbindingtoactivate"] = new() { ["en"] = "The minimum threshold in order for the assigned binding to activate.", ["zh"] = "触发绑定所需的最小阈值。", ["ko"] = "할당된 바인딩을 활성화하기 위한 최소 임계값입니다.", ["es"] = "Umbral mínimo para activar el vínculo asignado.", ["ru"] = "Минимальный порог для активации назначенной привязки.", ["fr"] = "Seuil minimum pour activer la liaison assignée.", ["de"] = "Mindestschwelle zur Aktivierung der zugewiesenen Bindung." },
            ["Theminimumthresholdindegreesinorderfortheassignedbindingtoactivate"] = new() { ["en"] = "The minimum threshold in degrees in order for the assigned binding to activate.", ["zh"] = "触发绑定所需的最小角度阈值。", ["ko"] = "할당된 바인딩을 활성화하기 위한 최소 각도 임계값입니다.", ["es"] = "Umbral mínimo en grados para activar el vínculo.", ["ru"] = "Минимальный порог в градусах для активации привязки.", ["fr"] = "Seuil minimum en degrés pour activer la liaison.", ["de"] = "Mindestschwelle in Grad zur Aktivierung der Bindung." },
            ["Disablepressureifitisavailable"] = new() { ["en"] = "Disable pressure if it is available", ["zh"] = "禁用压感（如果可用）", ["ko"] = "필압 비활성화 (가능한 경우)", ["es"] = "Desactivar presión si está disponible", ["ru"] = "Отключить давление, если доступно", ["fr"] = "Désactiver la pression si disponible", ["de"] = "Druck deaktivieren, falls verfügbar" },
            ["Disabletiltifitisavailable"] = new() { ["en"] = "Disable tilt if it is available", ["zh"] = "禁用倾斜（如果可用）", ["ko"] = "기울기 비활성화 (가능한 경우)", ["es"] = "Desactivar inclinación si está disponible", ["ru"] = "Отключить наклон, если доступен", ["fr"] = "Désactiver l'inclinaison si disponible", ["de"] = "Neigung deaktivieren, falls verfügbar" },
            ["PenBindingsrequirepressuretoactivate"] = new() { ["en"] = "Pen Bindings require pressure to activate", ["zh"] = "笔按键绑定需要压感激活", ["ko"] = "펜 바인딩은 활성화에 필압이 필요합니다", ["es"] = "Los vínculos de lápiz requieren presión", ["ru"] = "Привязки пера требуют давления", ["fr"] = "Les liaisons du stylet nécessitent une pression", ["de"] = "Stiftbindungen benötigen Druck" },
            ["Angleofrotationaboutthecenterofthearea"] = new() { ["en"] = "Angle of rotation about the center of the area.", ["zh"] = "围绕区域中心的旋转角度。", ["ko"] = "영역 중심을 기준으로 한 회전 각도입니다.", ["es"] = "Ángulo de rotación sobre el centro del área.", ["ru"] = "Угол поворота относительно центра области.", ["fr"] = "Angle de rotation autour du centre de la zone.", ["de"] = "Drehwinkel um die Mitte des Bereichs." },
            ["Youcanrightclicktheareaeditortoenableaspectratiolockingadjustalignmentorresizethearea"] = new() { ["en"] = "You can right click the area editor to enable aspect ratio locking, adjust alignment, or resize the area.", ["zh"] = "右键点击区域编辑器可锁定宽高比、调整对齐或缩放区域。", ["ko"] = "영역 편집기를 우클릭하여 종횡비를 잠그거나 정렬을 조정하거나 영역 크기를 조정할 수 있습니다.", ["es"] = "Clic derecho en el editor de área para bloquear aspecto, ajustar alineación o redimensionar.", ["ru"] = "ПКМ по редактору области для блокировки пропорций, выравнивания или изменения размера.", ["fr"] = "Clic droit sur l'éditeur de zone pour verrouiller le ratio, ajuster l'alignement ou redimensionner.", ["de"] = "Rechtsklick auf den Bereichseditor, um Seitenverhältnis zu sperren, Ausrichtung anzupassen oder Größe zu ändern." },
            ["Youcanrightclicktheareaeditortosettheareatoadisplayadjustalignmentorresizethearea"] = new() { ["en"] = "You can right click the area editor to set the area to a display, adjust alignment, or resize the area.", ["zh"] = "右键点击区域编辑器可将区域设为显示器、调整对齐或缩放区域。", ["ko"] = "영역 편집기를 우클릭하여 영역을 디스플레이로 설정하거나 정렬을 조정하거나 영역 크기를 조정할 수 있습니다.", ["es"] = "Clic derecho en el editor de área para asignar a pantalla, ajustar alineación o redimensionar.", ["ru"] = "ПКМ по редактору области, чтобы привязать к дисплею, выровнять или изменить размер.", ["fr"] = "Clic droit sur l'éditeur de zone pour l'associer à un écran, ajuster l'alignement ou redimensionner.", ["de"] = "Rechtsklick auf Bereichseditor, um Bereich einem Display zuzuweisen, Ausrichtung anzupassen oder Größe zu ändern." },
            ["Requestsallstringsinastringdumpeveniftheyareknowntolikelydamageordisruptusageofthetablet"] = new() { ["en"] = "Requests all strings in a string dump even if they are known to likely damage or disrupt usage of the tablet", ["zh"] = "请求导出所有字符串，即使已知某些字符串可能损坏或干扰数位板使用", ["ko"] = "태블릿 사용을 손상시키거나 방해할 수 있는 것으로 알려진 경우에도 문자열 덤프에서 모든 문자열을 요청합니다", ["es"] = "Solicita todas las cadenas incluso si pueden dañar la tableta", ["ru"] = "Запрашивает все строки, даже если они могут повредить планшет", ["fr"] = "Demande toutes les chaînes même si elles peuvent endommager la tablette", ["de"] = "Fordert alle Strings an, selbst wenn sie das Tablet beschädigen könnten" },
            ["Pausesstringdumpwithapopupboxifanystringdumperrorsoccur"] = new() { ["en"] = "Pauses string dump with a pop-up box if any string dump errors occur", ["zh"] = "如果导出字符串时发生错误，弹窗暂停", ["ko"] = "문자열 덤프 오류 발생 시 팝업 상자로 일시 중지합니다", ["es"] = "Pausa el volcado con ventana emergente si hay errores", ["ru"] = "Приостанавливает дамп с всплывающим окном при ошибках", ["fr"] = "Suspend le vidage avec une fenêtre contextuelle en cas d'erreur", ["de"] = "Pausiert String-Ausgabe mit Popup bei Fehlern" },
            ["WARNINGnArbitraryuseofthistoolmaycausedamageordisruptusageofyourtablet"] = new() { ["en"] = "WARNING\nArbitrary use of this tool may cause damage or disrupt usage of your tablet", ["zh"] = "警告\n随意使用此工具可能导致数位板损坏或使用异常", ["ko"] = "경고\n이 도구를 임의로 사용하면 태블릿이 손상되거나 사용이 중단될 수 있습니다", ["es"] = "ADVERTENCIA\nEl uso arbitrario puede dañar o interrumpir el uso de su tableta", ["ru"] = "ПРЕДУПРЕЖДЕНИЕ\nПроизвольное использование может повредить планшет", ["fr"] = "AVERTISSEMENT\nL'utilisation arbitraire peut endommager votre tablette", ["de"] = "WARNUNG\nWillkürliche Nutzung kann das Tablet beschädigen" },
            ["Nosupportedoutputmodeselected_1"] = new() { ["en"] = "No supported output mode selected", ["zh"] = "未选择支持的输出模式", ["ko"] = "지원되는 출력 모드가 선택되지 않음", ["es"] = "Modo de salida no seleccionado", ["ru"] = "Режим вывода не выбран", ["fr"] = "Aucun mode de sortie sélectionné", ["de"] = "Kein Ausgabemodus gewählt" },
            ["Notabletsaredetected_1"] = new() { ["en"] = "No tablets are detected", ["zh"] = "未检测到数位板", ["ko"] = "태블릿이 감지되지 않음", ["es"] = "No se detectan tabletas", ["ru"] = "Планшеты не обнаружены", ["fr"] = "Aucune tablette détectée", ["de"] = "Keine Tablets erkannt" },
            ["Notabletisselected"] = new() { ["en"] = "No tablet is selected!", ["zh"] = "未选择数位板！", ["ko"] = "태블릿이 선택되지 않았습니다!", ["es"] = "¡Ninguna tableta seleccionada!", ["ru"] = "Планшет не выбран!", ["fr"] = "Aucune tablette sélectionnée !", ["de"] = "Kein Tablet ausgewählt!" },
            ["Notabletdetected"] = new() { ["en"] = "No tablet detected!", ["zh"] = "未检测到数位板！", ["ko"] = "태블릿이 감지되지 않았습니다!", ["es"] = "¡Ninguna tableta detectada!", ["ru"] = "Планшет не обнаружен!", ["fr"] = "Aucune tablette détectée !", ["de"] = "Kein Tablet erkannt!" },
            ["Nosupportedoutputmodeselected_2"] = new() { ["en"] = "No supported output mode selected!", ["zh"] = "未选择支持的输出模式！", ["ko"] = "지원되는 출력 모드가 선택되지 않았습니다!", ["es"] = "¡Modo de salida no seleccionado!", ["ru"] = "Режим вывода не выбран!", ["fr"] = "Aucun mode de sortie sélectionné !", ["de"] = "Kein Ausgabemodus gewählt!" },
            ["Memo"] = new() { ["en"] = "Memo", ["zh"] = "悼念", ["ko"] = "추모", ["es"] = "Conmemoración", ["ru"] = "Память", ["fr"] = "Mémo", ["de"] = "Gedenken" },
            ["File"] = new() { ["en"] = "&File", ["zh"] = "文件(&F)", ["ko"] = "파일(&F)", ["es"] = "&Archivo", ["ru"] = "&Файл", ["fr"] = "&Fichier", ["de"] = "&Datei" },
            ["Tablets"] = new() { ["en"] = "Tablets", ["zh"] = "数位板", ["ko"] = "태블릿", ["es"] = "Tabletas", ["ru"] = "Планшеты", ["fr"] = "Tablettes", ["de"] = "Tablets" },
            ["Help"] = new() { ["en"] = "&Help", ["zh"] = "帮助(&H)", ["ko"] = "도움말(&H)", ["es"] = "A&yuda", ["ru"] = "&Справка", ["fr"] = "Aid&e", ["de"] = "&Hilfe" },
            ["Plugins1"] = new() { ["en"] = "Plugins_1", ["zh"] = "插件", ["ko"] = "플러그인", ["es"] = "Complementos", ["ru"] = "Плагины", ["fr"] = "Extensions", ["de"] = "Plugins" },
            ["LanguageMenu"] = new() { ["en"] = "LanguageMenu", ["zh"] = "语言", ["ko"] = "언어", ["es"] = "Idioma", ["ru"] = "Язык", ["fr"] = "Langue", ["de"] = "Sprache" },
        };
    }
}