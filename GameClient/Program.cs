using System;

namespace VagabondRL
{
    class Program
    {
        static void Main(string[] args)
        {
#if RELEASE
            try
            {
#endif
            using var game = new Game();
            game.Run();
#if RELEASE
            }
            catch (Exception ex)
            {
                ElementEngine.Logging.Information(ex.ToString());
            }
#endif
        }
    }
}
