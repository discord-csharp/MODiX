using System;
using System.Collections.Generic;

namespace Modix.Services.CodePaste
{
    public class MemoryCodePasteRepository : ICodePasteRepository
    {
        private readonly List<UserCodePaste> _pastes = new List<UserCodePaste>();

        public MemoryCodePasteRepository()
        {
            _pastes.Add(new UserCodePaste
            {
                Id = 0,
                ChannelName = "bots",
                CreatorUsername = "jmazouri",
                Created = DateTime.UtcNow,
                Content = "public static void Main()" +
                "\n{" +
                "\n    public int Test {get; set;}" +
                "\n    public string Something {get; set;}" +
                "\n}"
            });
            _pastes.Add(new UserCodePaste
            {
                Id = 1,
                ChannelName = "bots",
                CreatorUsername = "jmazouri",
                Created = DateTime.UtcNow,
                Content = "public static void Main()" +
                "\n{" +
                "\n    public int Test {get; set;}" +
                "\n    public string Something {get; set;}" +
                "\n}"
            });
            _pastes.Add(new UserCodePaste
            {
                Id = 2,
                ChannelName = "bots",
                CreatorUsername = "jmazouri",
                Created = DateTime.UtcNow,
                Content = "public static void Main()" +
                "\n{" +
                "\n    public int Test {get; set;}" +
                "\n    public string Something {get; set;}" +
                "\n}"
            });
        }

        public void AddPaste(UserCodePaste paste)
        {
            _pastes.Add(paste);
        }

        public UserCodePaste GetPaste(int id)
        {
            return _pastes.Find(d => d.Id == id);
        }

        public IEnumerable<UserCodePaste> GetPastes()
        {
            return _pastes.AsReadOnly();
        }
    }
}
