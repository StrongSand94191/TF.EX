﻿using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using TF.EX.Domain;
using TF.EX.Domain.Extensions;
using TF.EX.Domain.Ports;
using TF.EX.Domain.Ports.TF;
using TF.EX.TowerFallExtensions;
using TowerFall;

namespace TF.EX.Patchs.PlayerInput
{
     //TODO: refactor
    public class XGamePadInputPatch : IHookable
    {
        private readonly IInputService _inputService;
        private INetplayManager _netplayManager;

        private delegate bool MenuConfirm_orig(XGamepadInput self);
        private delegate bool MenuBack_orig(XGamepadInput self);
        private delegate bool MenuLeft_orig(XGamepadInput self);
        private delegate bool MenuRight_orig(XGamepadInput self);
        private delegate bool MenuUp_orig(XGamepadInput self);
        private delegate bool MenuDown_orig(XGamepadInput self);
        private delegate bool MenuAlt_orig(XGamepadInput self);
        private delegate bool MenuSkipReplay_orig(XGamepadInput self);
        private delegate bool MenuSaveReplay_orig(XGamepadInput self);
        private delegate bool MenuSaveReplayCheck_orig(XGamepadInput self);
        private static IDetour MenuConfirm_hook;
        private static IDetour MenuBack_hook;
        private static IDetour MenuLeft_hook;
        private static IDetour MenuRight_hook;
        private static IDetour MenuUp_hook;
        private static IDetour MenuDown_hook;
        private static IDetour MenuAlt_hook;
        private static IDetour MenuSkipReplay_hook;
        private static IDetour MenuSaveReplay_hook;
        private static IDetour MenuSaveReplayCheck_hook;


        public XGamePadInputPatch(IInputService inputService, INetplayManager netplayManager)
        {
            _inputService = inputService;
            _netplayManager = netplayManager;
        }

        public void Load()
        {
            On.TowerFall.XGamepadInput.GetState += XGamepadInput_GetState;

            MenuConfirm_hook = new Hook(typeof(XGamepadInput).GetProperty("MenuConfirm").GetGetMethod(), MenuConfirm_patch);
            MenuBack_hook = new Hook(typeof(XGamepadInput).GetProperty("MenuBack").GetGetMethod(), MenuBack_patch);
            MenuLeft_hook = new Hook(typeof(XGamepadInput).GetProperty("MenuLeft").GetGetMethod(), MenuLR_patch);
            MenuRight_hook = new Hook(typeof(XGamepadInput).GetProperty("MenuRight").GetGetMethod(), MenuLR_patch);
            MenuUp_hook = new Hook(typeof(XGamepadInput).GetProperty("MenuUp").GetGetMethod(), MenuUp_patch);
            MenuDown_hook = new Hook(typeof(XGamepadInput).GetProperty("MenuDown").GetGetMethod(), MenuDown_patch);
            MenuAlt_hook = new Hook(typeof(XGamepadInput).GetProperty("MenuAlt").GetGetMethod(), MenuConfirm_patch);
            MenuSkipReplay_hook = new Hook(typeof(XGamepadInput).GetProperty("MenuSkipReplay").GetGetMethod(), MenuSkip_patch);
            MenuSaveReplay_hook = new Hook(typeof(XGamepadInput).GetProperty("MenuSaveReplay").GetGetMethod(), MenuSave_patch);
            MenuSaveReplayCheck_hook = new Hook(typeof(XGamepadInput).GetProperty("MenuSaveReplayCheck").GetGetMethod(), MenuSave_patch);
        }

        public void Unload()
        {
            On.TowerFall.XGamepadInput.GetState -= XGamepadInput_GetState;

            MenuConfirm_hook.Dispose();
            MenuBack_hook.Dispose();
            MenuLeft_hook.Dispose();
            MenuRight_hook.Dispose();
            MenuAlt_hook.Dispose();
            MenuSkipReplay_hook.Dispose();
            MenuSaveReplay_hook.Dispose();
            MenuSaveReplayCheck_hook.Dispose();
        }

        private static bool MenuConfirm_patch(MenuConfirm_orig orig, XGamepadInput self)
        {
            return InterceptConfirm(self, orig(self));
        }

        private static bool MenuBack_patch(MenuConfirm_orig orig, XGamepadInput self)
        {
            return InterceptBack(self, orig(self));
        }

        private static bool MenuLR_patch(MenuConfirm_orig orig, XGamepadInput self)
        {
            return InterceptLR(self, orig(self));
        }

        private static bool MenuUp_patch(MenuConfirm_orig orig, XGamepadInput self)
        {
            var inputService = ServiceCollections.ResolveInputService();

            if (TFGame.Instance.Scene is MainMenu
               && TowerFall.MainMenu.VersusMatchSettings.Mode.ToModel().IsNetplay()
               && inputService.GetInputIndex(self) != 0)
            {
                return false;
            }

            return orig(self);
        }

        private static bool MenuDown_patch(MenuConfirm_orig orig, XGamepadInput self)
        {
            var inputService = ServiceCollections.ResolveInputService();

            if (TFGame.Instance.Scene is MainMenu
                && TowerFall.MainMenu.VersusMatchSettings.Mode.ToModel().IsNetplay()
                && inputService.GetInputIndex(self) != 0)
            {
                return false;
            }

            return orig(self);
        }

        private static bool MenuSkip_patch(MenuConfirm_orig orig, XGamepadInput self)
        {
            return true;
        }

        private static bool MenuSave_patch(MenuConfirm_orig orig, XGamepadInput self)
        {
            return false;
        }

        private InputState XGamepadInput_GetState(On.TowerFall.XGamepadInput.orig_GetState orig, XGamepadInput self)
        {

            if (_netplayManager.GetNetplayMode() != Domain.Models.NetplayMode.Test
                && _netplayManager.GetNetplayMode() != Domain.Models.NetplayMode.Replay
                && !_netplayManager.IsSynchronized())
            {
                return orig(self);
            }

            var level = TFGame.Instance.Scene as TowerFall.Level;

            if (level == null)
            {
                return orig(self);
            }

            var roundStarted = level.Session.RoundLogic.RoundStarted;

            if (!roundStarted) //Ignore if round not started to prevent useless rollback
            {
                _inputService.ResetPolledInput();
                return new InputState();
            }

            if (_netplayManager.IsInit() || _netplayManager.IsReplayMode())
            {
                var polledInput = orig(self);

                if (IsLocalPlayerGamePad(self))
                {
                    if (!_netplayManager.IsReplayMode())
                    {
                        _inputService.UpdatePolledInput(polledInput);
                    }
                    else
                    {
                        //InterceptReplay(polledInput); TODO: implement replay interception
                    }
                    return _inputService.GetCurrentInput(_inputService.GetLocalPlayerInputIndex());
                }

                return _inputService.GetCurrentInput(_inputService.GetRemotePlayerInputIndex());
            }
            else
            {
                return orig(self);
            }
        }

        //TODO: refactor to have a unique intercept for all inputs
        private static bool InterceptConfirm(XGamepadInput self, bool actualInput)
        {
            (var state_machine, var mode) = ServiceCollections.ResolveStateMachineService();
            var netplayManager = ServiceCollections.ResolveNetplayManager();
            var inputService = ServiceCollections.ResolveInputService();

            var init = state_machine.IsInitialized();
            var canStart = state_machine.CanStart();
            var isNetplayInit = netplayManager.IsInit();
            var isReplayMode = netplayManager.IsReplayMode();
            var isPaused = TFGame.Instance.Scene is TowerFall.Level && (TFGame.Instance.Scene as TowerFall.Level).Paused;

            if (TFGame.Instance.Scene is MainMenu
                && TowerFall.MainMenu.VersusMatchSettings.Mode.ToModel().IsNetplay()
                && inputService.GetInputIndex(self) != 0)
            {
                return false; //Ignore input for other players in netplay
            }

            if (TFGame.Instance.Scene is MapScene)
            {
                return true;
            }

            if (isPaused)
            {
                return actualInput;
            }

            if (TFGame.Instance.Scene is TowerFall.Level && (TFGame.Instance.Scene as TowerFall.Level).Session.GetWinner() != -1)
            {
                var dynMacthResults = DynamicData.For((TFGame.Instance.Scene as TowerFall.Level).Get<VersusMatchResults>());
                var isFinished = dynMacthResults.Get<bool>("finished");

                if (isFinished)
                {
                    return actualInput;
                }
            }


            if (TFGame.Instance.Scene is MainMenu)
            {
                var dynMenu = DynamicData.For(TFGame.Instance.Scene as MainMenu);
                var state = dynMenu.Get<MainMenu.MenuState>("state");

                if (state == MainMenu.MenuState.Rollcall && mode.IsNetplay())
                {
                    if (canStart)
                    {
                        return true;
                    }

                    if (!init)
                    {
                        return false;
                    }
                }
            }

            if (isNetplayInit)
            {
                return true;
            }

            if (netplayManager.IsDisconnected())
            {
                return actualInput;
            }

            if (isReplayMode)
            {
                return true;
            }

            if (canStart)
            {
                return true;
            }

            return actualInput;
        }

        private static bool InterceptBack(XGamepadInput self, bool actualInput)
        {
            try
            {
                (var state_machine, _) = ServiceCollections.ResolveStateMachineService();
                var netplayManager = ServiceCollections.ResolveNetplayManager();
                var inputService = ServiceCollections.ResolveInputService();

                var init = state_machine.IsInitialized();
                var canStart = state_machine.CanStart();
                var isNetplayInit = netplayManager.IsInit();
                var isPaused = TFGame.Instance.Scene is TowerFall.Level && (TFGame.Instance.Scene as TowerFall.Level).Paused;

                if (TFGame.Instance.Scene is MainMenu
                && TowerFall.MainMenu.VersusMatchSettings.Mode.ToModel().IsNetplay()
                && inputService.GetInputIndex(self) != 0)
                {
                    return false; //Ignore input for other players in netplay
                }

                if (isPaused)
                {
                    return actualInput;
                }

                if (TFGame.Instance.Scene is TowerFall.Level && (TFGame.Instance.Scene as TowerFall.Level).Session.GetWinner() != -1)
                {
                    var dynMacthResults = DynamicData.For((TFGame.Instance.Scene as TowerFall.Level).Get<VersusMatchResults>());
                    var isFinished = dynMacthResults.Get<bool>("finished");

                    if (isFinished)
                    {
                        return actualInput;
                    }
                }

                if (TFGame.Instance.Scene is MainMenu)
                {
                    var dynMenu = DynamicData.For(TFGame.Instance.Scene as MainMenu);
                    var state = dynMenu.Get<MainMenu.MenuState>("state");

                    if (state == MainMenu.MenuState.Rollcall)
                    {
                        if (canStart)
                        {
                            return false;
                        }

                        if (init)
                        {
                            return false;
                        }
                    }
                }

                if (TFGame.Instance.Scene is MapScene)
                {
                    return false;
                }

                if (isNetplayInit)
                {
                    return true;
                }

                return actualInput;
            }
            catch
            {
                // The game is probably still loading here, so we don't care about the exception
                return actualInput;
            }
        }

        private static bool InterceptLR(XGamepadInput self, bool actualInput)
        {
            (var state_machine, _) = ServiceCollections.ResolveStateMachineService();
            var netplayManager = ServiceCollections.ResolveNetplayManager();
            var inputService = ServiceCollections.ResolveInputService();

            var init = state_machine.IsInitialized();
            var canStart = state_machine.CanStart();
            var isNetplayInit = netplayManager.IsInit();
            var isPaused = TFGame.Instance.Scene is TowerFall.Level && (TFGame.Instance.Scene as TowerFall.Level).Paused;

            if (TFGame.Instance.Scene is MainMenu
                && TowerFall.MainMenu.VersusMatchSettings.Mode.ToModel().IsNetplay()
                && inputService.GetInputIndex(self) != 0)
            {
                return false;
            }

            if (isPaused)
            {
                return actualInput;
            }

            if (TFGame.Instance.Scene is MainMenu)
            {
                var dynMenu = DynamicData.For(TFGame.Instance.Scene as MainMenu);
                var state = dynMenu.Get<MainMenu.MenuState>("state");

                if (state == MainMenu.MenuState.Rollcall)
                {
                    if (canStart)
                    {
                        return false;
                    }

                    if (!init)
                    {
                        return false;
                    }
                }
            }

            if (TFGame.Instance.Scene is MapScene)
            {
                return false;
            }

            if (isNetplayInit)
            {
                return true;
            }

            return actualInput;
        }

        private bool IsLocalPlayerGamePad(XGamepadInput self)
        {
            return _inputService.GetInputIndex(self) == 0;
        }
    }
}
