using System;
namespace Otrs2Alfresco
{
    public interface ITargetApi
    {
        ITargetCase OpenOrCreateCase(string caseName);
    }

    public interface ITargetCase
    { 
        bool FileExists(string prefix);

        void CreateFile(string filename, byte[] filedata);
    }
}
