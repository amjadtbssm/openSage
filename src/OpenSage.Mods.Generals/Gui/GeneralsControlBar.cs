﻿using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Logic;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Generals.Gui
{
    public sealed class GeneralsControlBar : IControlBar
    {
        private enum ControlBarSize
        {
            Maximized,
            Minimized
        }

        // How much the control bar should be moved down when minimized?
        private const int MinimizeOffset = 120;

        private readonly ContentManager _contentManager;

        private readonly ControlBarScheme _scheme;

        private ControlBarState _state;

        private ControlBarState State
        {
            get => _state;
            set
            {
                _state = value;
                _state.OnEnterState(this);
            }
        }

        private readonly Window _background;
        private readonly Window _window;

        private readonly Control _center;
        private readonly Control _right;

        private readonly Label _moneyDisplay;
        // TODO: Change this to a ProgressBar when they are implemented.
        private readonly Control _powerBar;
        // TODO: Change this to a ProgressBar when they are implemented.
        private readonly Control _expBar;

        private readonly Button _resize;

        private readonly Image _resizeDownBackground;
        private readonly Image _resizeDownHover;
        private readonly Image _resizeDownPushed;

        private readonly Image _resizeUpBackground;
        private readonly Image _resizeUpHover;
        private readonly Image _resizeUpPushed;

        private ControlBarSize _size = ControlBarSize.Maximized;

        private Image LoadImage(string name) => _contentManager.WndImageLoader.CreateNormalImage(name);
        private Control FindControl(string name) => _window.Controls.FindControl($"ControlBar.wnd:{name}");

        public GeneralsControlBar(Window background, Window window, ControlBarScheme scheme, ContentManager contentManager)
        {
            _background = background;
            _window = window;
            _scheme = scheme;
            _contentManager = contentManager;

            _center = FindControl("CenterBackground");
            _right = FindControl("RightHUD");

            _moneyDisplay = FindControl("MoneyDisplay") as Label;
            _moneyDisplay.Text = "$ 0";
            _powerBar = FindControl("PowerWindow");
            _expBar = FindControl("GeneralsExp");

            _resize = FindControl("ButtonLarge") as Button;

            _resizeDownBackground = LoadImage(_scheme.ToggleButtonDownOn);
            _resizeDownHover = LoadImage(_scheme.ToggleButtonDownIn);
            _resizeDownPushed = LoadImage(_scheme.ToggleButtonDownPushed);

            _resizeUpBackground = LoadImage(_scheme.ToggleButtonUpOn);
            _resizeUpHover = LoadImage(_scheme.ToggleButtonUpIn);
            _resizeUpPushed = LoadImage(_scheme.ToggleButtonUpPushed);

            UpdateResizeButtonStyle();

            State = ControlBarState.Default;
        }

        // TODO: This should be called at every logic tick.
        // TODO: This takes a player as the state information. Do we need any other state?
        public void Update(Player player)
        {
            _moneyDisplay.Text = $"$ {player.Money}";
            State.Update(player, this);
        }

        public void ToggleSize()
        {
            if (_size == ControlBarSize.Maximized)
            {
                _window.Top += MinimizeOffset;
                _background.Top += MinimizeOffset;
                _size = ControlBarSize.Minimized;
            }
            else
            {
                _window.Top -= MinimizeOffset;
                _background.Top -= MinimizeOffset;
                _size = ControlBarSize.Maximized;
            }

            UpdateResizeButtonStyle();
        }

        private void UpdateResizeButtonStyle()
        {
            if (_size == ControlBarSize.Maximized)
            {
                _resize.BackgroundImage = _resizeDownBackground;
                _resize.HoverBackgroundImage = _resizeDownHover;
                _resize.PushedBackgroundImage = _resizeDownPushed;
            }
            else
            {
                _resize.BackgroundImage = _resizeUpBackground;
                _resize.HoverBackgroundImage = _resizeUpHover;
                _resize.PushedBackgroundImage = _resizeUpPushed;
            }
        }

        public void AddToScene(Scene2D scene2D)
        {
            scene2D.WndWindowManager.PushWindow(_background);
            scene2D.WndWindowManager.PushWindow(_window);
        }

        private abstract class ControlBarState
        {
            public abstract void OnEnterState(GeneralsControlBar controlBar);
            public abstract void Update(Player player, GeneralsControlBar controlBar);

            public static ControlBarState Default { get; } = new DefaultControlBarState();
        }

        private sealed class DefaultControlBarState : ControlBarState
        {
            public override void OnEnterState(GeneralsControlBar controlBar)
            {
                foreach (var control in controlBar._center.Controls)
                {
                    control.Hide();
                }

                foreach (var control in controlBar._right.Controls)
                {
                    control.Hide();
                }
            }

            public override void Update(Player player, GeneralsControlBar controlBar)
            {

            }
        }

        private sealed class SelectedControlBarState : ControlBarState
        {
            public override void OnEnterState(GeneralsControlBar controlBar)
            {
                throw new System.NotImplementedException();
            }

            public override void Update(Player player, GeneralsControlBar controlBar)
            {
                throw new System.NotImplementedException();
            }
        }

        private sealed class UnderConstructionControlBarState : ControlBarState
        {
            public override void OnEnterState(GeneralsControlBar controlBar)
            {
                throw new System.NotImplementedException();
            }

            public override void Update(Player player, GeneralsControlBar controlBar)
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public sealed class GeneralsControlBarSource : IControlBarSource
    {
        public IControlBar Create(string side, ContentManager contentManager)
        {
            // TODO: This is not the best place for this.
            contentManager.IniDataContext.LoadIniFile(@"Data\INI\ControlBarScheme.ini");

            var scheme = contentManager.IniDataContext.ControlBarSchemes.FindBySide(side);

            // TODO: Support multiple image parts?
            // Generals always uses exactly one image part.
            var imagePart = scheme.ImageParts[0];

            var background = new Control
            {
                Name = "OpenSAGE:ControlBarBackground",
                Bounds = new Rectangle(imagePart.Position, imagePart.Size),
                BackgroundImage = LoadImage(imagePart.ImageName)
            };

            var backgroundWindow = new Window(scheme.ScreenCreationRes, background, contentManager);
            var controlBarWindow = contentManager.Load<Window>("Window/ControlBar.wnd", new LoadOptions { CacheAsset = false });

            Control FindControl(string name) => controlBarWindow.Controls.FindControl($"ControlBar.wnd:{name}");
            Image LoadImage(string path) => contentManager.WndImageLoader.CreateNormalImage(path);

            // TODO: Implement under attack indicator.
            FindControl("WinUAttack").Hide();

            // TODO: What is this?
            FindControl("OnTopDraw").Hide();

            var windowOrigin = controlBarWindow.Bounds.Location;
            var schemeType = scheme.GetType();

            Control ApplyBounds(string name, string coordPrefix)
            {
                var control = FindControl(name);

                var ul = (Point2D) schemeType.GetProperty($"{coordPrefix}UL").GetValue(scheme);
                var lr = (Point2D) schemeType.GetProperty($"{coordPrefix}LR").GetValue(scheme);
                control.Bounds = Rectangle.FromCorners(ul - windowOrigin, lr - windowOrigin);

                return control;
            }

            void ApplyButtonScheme(string name, string coordPrefix, string texturePrefix)
            {
                var button = ApplyBounds(name, coordPrefix) as Button;

                Image LoadImageForState(string state) =>
                    LoadImage(
                        (string) schemeType.GetProperty($"{texturePrefix}{state}")?.GetValue(scheme));

                button.BackgroundImage = LoadImageForState("Enable");
                button.DisabledBackgroundImage = LoadImageForState("Disabled");
                button.HoverBackgroundImage = LoadImageForState("Highlighted");
                button.PushedBackgroundImage = LoadImageForState("Pushed");
            }

            ApplyBounds("MoneyDisplay", "Money");
            ApplyBounds("PowerWindow", "PowerBar");

            ApplyButtonScheme("ButtonOptions", "Options", "OptionsButton");
            ApplyButtonScheme("ButtonPlaceBeacon", "Beacon", "BeaconButton");
            ApplyButtonScheme("PopupCommunicator", "Chat", "BuddyButton");
            ApplyButtonScheme("ButtonIdleWorker", "Worker", "IdleWorkerButton");

            ApplyButtonScheme("ButtonGeneral", "General", "GeneralButton");
            // Textures are set by ControlBar
            ApplyBounds("ButtonLarge", "MinMax");

            // TODO: Hide left HUD until we implement the minimap.
            FindControl("LeftHUD").Hide();

            var rightHud = FindControl("RightHUD");
            rightHud.BorderWidth = 0;
            rightHud.BackgroundColor = ColorRgbaF.Transparent;
            rightHud.BackgroundImage = LoadImage(scheme.RightHudImage);

            FindControl("ExpBarForeground").BackgroundImage = LoadImage(scheme.ExpBarForegroundImage);

            return new GeneralsControlBar(backgroundWindow, controlBarWindow, scheme, contentManager);
        }
    }
}
