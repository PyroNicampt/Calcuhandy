﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SharpHook;

namespace Calcuhandy {
    internal class ProgramHotkeys {
        public static ProgramHotkeys manager {
            get => Init();
        }
        private static ProgramHotkeys? _main;
        private enum HotkeyAction { Open, Hide };
        public event EventHandler HotkeyOpen = delegate { };
        public event EventHandler HotkeyHide = delegate { };

        private TaskPoolGlobalHook hook;
        private EventSimulator sim;
        private enum KeyMods {
            None = 0, Ctrl = 1, Alt = 2, Shift = 4, Meta = 8
        };
        private int KeyMod_Ctrl = 0;
        private int KeyMod_Alt = 0;
        private int KeyMod_Shift = 0;
        private int KeyMod_Meta = 0;

        public static ProgramHotkeys Init() {
            _main ??= new ProgramHotkeys();
            return _main;
        }
        public ProgramHotkeys() {
            hook = new(runAsyncOnBackgroundThread: true, globalHookType:GlobalHookType.Keyboard);
            sim = new();
            hook.KeyPressed += new EventHandler<KeyboardHookEventArgs>(KeyDown);
            hook.KeyReleased += new EventHandler<KeyboardHookEventArgs>(KeyUp);
            hook.RunAsync();
        }
        private void KeyDown(object? source, KeyboardHookEventArgs args) {
            switch(args.Data.KeyCode) {
                case SharpHook.Native.KeyCode.VcLeftControl:
                case SharpHook.Native.KeyCode.VcRightControl:
                    KeyMod_Ctrl = Math.Min(KeyMod_Ctrl + 1, 1);
                    break;
                case SharpHook.Native.KeyCode.VcLeftShift:
                case SharpHook.Native.KeyCode.VcRightShift:
                    KeyMod_Shift = Math.Min(KeyMod_Shift + 1, 1);
                    break;
                case SharpHook.Native.KeyCode.VcLeftAlt:
                case SharpHook.Native.KeyCode.VcRightAlt:
                    KeyMod_Alt = Math.Min(KeyMod_Alt + 1, 1);
                    break;
                case SharpHook.Native.KeyCode.VcLeftMeta:
                case SharpHook.Native.KeyCode.VcRightMeta:
                    KeyMod_Meta = Math.Min(KeyMod_Meta + 1, 1);
                    break;
                case SharpHook.Native.KeyCode.VcC:
                    if(CheckKeyMods(KeyMods.Meta)) HotkeyTriggered(HotkeyAction.Open);
                    break;
                case SharpHook.Native.KeyCode.VcEscape:
                    if(CheckKeyMods(KeyMods.None)) HotkeyTriggered(HotkeyAction.Hide);
                    break;
            }
        }
        private void KeyUp(object? source, KeyboardHookEventArgs args) {
            switch(args.Data.KeyCode) {
                case SharpHook.Native.KeyCode.VcLeftControl:
                case SharpHook.Native.KeyCode.VcRightControl:
                    KeyMod_Ctrl = Math.Max(KeyMod_Ctrl - 1, 0);
                    break;
                case SharpHook.Native.KeyCode.VcLeftShift:
                case SharpHook.Native.KeyCode.VcRightShift:
                    KeyMod_Shift = Math.Max(KeyMod_Shift - 1, 0);
                    break;
                case SharpHook.Native.KeyCode.VcLeftAlt:
                case SharpHook.Native.KeyCode.VcRightAlt:
                    KeyMod_Alt = Math.Max(KeyMod_Alt - 1, 0);
                    break;
                case SharpHook.Native.KeyCode.VcLeftMeta:
                case SharpHook.Native.KeyCode.VcRightMeta:
                    KeyMod_Meta = Math.Max(KeyMod_Meta - 1, 0);
                    break;
            }
        }
        private bool CheckKeyMods(KeyMods keyMods) {
            int mask = 0;
            if(KeyMod_Ctrl > 0) mask |= 1;
            if(KeyMod_Alt > 0) mask |= 2;
            if(KeyMod_Shift > 0) mask |= 4;
            if(KeyMod_Meta > 0) mask |= 8;

            return (KeyMods)mask == keyMods;
        }
        private void HotkeyTriggered(HotkeyAction action) {
            switch(action) {
                case HotkeyAction.Open:
                    HotkeyOpen(this, EventArgs.Empty);
                    break;
                case HotkeyAction.Hide:
                    HotkeyHide(this, EventArgs.Empty);
                    break;
            }
        }
        public void SimulateAltKeyTap() {
            sim.SimulateKeyPress(SharpHook.Native.KeyCode.VcLeftAlt);
            sim.SimulateKeyRelease(SharpHook.Native.KeyCode.VcLeftAlt);
        }
    }
}
