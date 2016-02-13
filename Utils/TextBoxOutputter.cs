using System;
using System.IO;
using System.Text;
using System.Windows.Controls;

namespace CoD2_Launcher.Utils
{
    public class TextBoxOutputter : TextWriter
    {
        TextBox textBox = null;

        public TextBoxOutputter(TextBox output)
        {
            textBox = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            textBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value.ToString());
                textBox.ScrollToEnd();
            }));
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}