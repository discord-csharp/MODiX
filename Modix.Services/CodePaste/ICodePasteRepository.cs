using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Services.CodePaste
{
    public interface ICodePasteRepository
    {
        UserCodePaste GetPaste(int id);
        IEnumerable<UserCodePaste> GetPastes();
        void AddPaste(UserCodePaste paste);
    }
}
