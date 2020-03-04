
namespace biped
{
    public class Config
    {
        public uint Left { get; set; }
        public uint Middle { get; set; }
        public uint Right { get; set; }

        public Config(uint left, uint middle, uint right)
        {
            Left = left;
            Middle = middle;
            Right = right;
        }
    }
}