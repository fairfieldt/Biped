
namespace biped
{
    public class Config
    {
        public uint Left { get; set; }
        public uint Middle { get; set; }
        public uint Right { get; set; }

        public enum MOUSE_BUTTON_CODES { Left = 1000, Middle = 2000, Right = 3000 };

        public Config(uint left, uint middle, uint right)
        {
            Left = left;
            Middle = middle;
            Right = right;
        }
    }
}