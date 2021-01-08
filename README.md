# Biped
simple Windows keyboard emulation for Infinity Foot Controls


The existing Pedable application lacks some features (multiple pedals down at once) and has some issues (dropping pedal up events).

This is a super simple alternative for those who want to send keyboard input with their foot pedals.

Pedals can also be configured to send left, right, or middle mouse click events.

Run Biped.exe and use the GUI to configure your pedals.  Configuration is stored in the registry.  Biped will run in the system tray; to
exit right click on the tray icon.


## Command Line Options

The application also supports command line parameters to configure your pedals when launching the application.

```biped2.exe -left [keycode] -middle [keycode] -right [keycode]```

The parameters can be in any order but all three pedals must be mapped to a value.

Key codes can be obtained by viewing the values displayed in the GUI.
