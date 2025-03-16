### Mouse modifier
This utility maps alphanumerical keys to work as function keys when mouse XButton2 is pressed.

I was running of keybinds to use and with recently changing my keyboard, that has different
function keys layout, I needed to find a way to use the function keys without more complicated
modifiers.

Mapped keys:
- `1` - `F1`
- `2` - `F2`
- `3` - `F3`
- `4` - `F4`
- `5` - `F5`
- `6` - `F6`
- `7` - `F7`
- `8` - `F8`
- `9` - `F9`
- `0` - `F10`

### Installation
Mouse modifier is published as single file console application. On win-x64 it's self contain,
so just double click and run it.

### Note
This was a nice find, but when implementing the `INPUT` struct, it is important to implement 
all three of the inputs - `KEYBDINPUT`, `MOUSEINPUT`, and `HARDWAREINPUT`. They are grouped 
together using union but they don't have the same size, so you either add padding to the one 
you need (in my case `KEYBDINPUT`) or implement all of them.

Also for the `INPUT` struct to correctly trigger the key press, both the `wVk` and `wScan` 
fields must be set. The `wVk` field is the virtual key code, and the `wScan` field 
is the hardware scan code. There is a table of scan codes, or you can use `MapVirtualKeyA`
or `MapVirtualKeyW` to get the scan code from the virtual key code.