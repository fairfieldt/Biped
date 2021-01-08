using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace biped
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();

            if (e.Args.Length > 0)
            {
                var cliBindings = ProcessCommandLineArguments(e.Args);
                if (cliBindings.Length == 3)
                {
                    wnd.ApplyCommandLineBindings(cliBindings[0], cliBindings[1], cliBindings[2]);
                }
            }
            wnd.Show();
        }

        private uint[] ProcessCommandLineArguments(string[] args)
        {

            uint leftBinding = uint.MaxValue;
            uint middleBinding = uint.MaxValue;
            uint rightBinding = uint.MaxValue;
            int argIndex = 0;
            
            try
            {
                while (argIndex < args.Length)
                {
                    switch (args[argIndex])
                    {
                        case "-left":
                            argIndex++;
                            leftBinding = uint.Parse(args[argIndex]);
                            break;
                        case "-middle":
                            argIndex++;
                            middleBinding = uint.Parse(args[argIndex]);
                            break;
                        case "-right":
                            argIndex++;
                            rightBinding = uint.Parse(args[argIndex]);
                            break;
                        default:
                            throw new Exception();
                    }
                    argIndex++;
                }

                if (leftBinding == uint.MaxValue || middleBinding == uint.MaxValue || rightBinding == uint.MaxValue ) {
                    MessageBox.Show("Must provide bindings for all three pedals!");
                    return new uint[0];
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Binding values must be the integer key code!");
                return new uint[0];
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid Command Line Parameters!");
                return new uint[0];
            }

            return new uint[]{ leftBinding, middleBinding, rightBinding };
        }
    }
}
