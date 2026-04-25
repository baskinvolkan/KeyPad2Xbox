using System;
using System.Collections.Generic;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace KeyPad2Xbox.Core
{
    public class KeyMapper
    {
        // Tuş eşleştirmelerini (ScanCode, IsExtended) şeklinde anahtarlıyoruz.
        // Value olarak ise gamepad üzerinde çalışacak fonksiyonu tutuyoruz.
        private readonly Dictionary<(ushort scanCode, bool extended), Action<IXbox360Controller, bool>> _map;

        public KeyMapper()
        {
            _map = new Dictionary<(ushort scanCode, bool extended), Action<IXbox360Controller, bool>>();

            // Yön tuşları (Extended = true)
            _map[(0x48, true)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.Up, pressed);
            _map[(0x50, true)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.Down, pressed);
            _map[(0x4B, true)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.Left, pressed);
            _map[(0x4D, true)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.Right, pressed);

            // Harf tuşları (Extended = false)
            _map[(0x10, false)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.LeftShoulder, pressed); // Q
            _map[(0x11, false)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.Y, pressed); // W
            _map[(0x12, false)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.RightShoulder, pressed); // E
            _map[(0x13, false)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.RightThumb, pressed); // R
            _map[(0x1E, false)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.X, pressed); // A
            _map[(0x1F, false)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.A, pressed); // S
            _map[(0x20, false)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.B, pressed); // D
            _map[(0x21, false)] = (pad, pressed) => pad.SetButtonState(Xbox360Button.LeftThumb, pressed); // F
        }

        public bool TryMap(ushort scanCode, bool isExtended, bool pressed, IXbox360Controller pad)
        {
            if (_map.TryGetValue((scanCode, isExtended), out var action))
            {
                action(pad, pressed);
                return true;
            }
            return false;
        }
    }
}
