using System.Collections.Generic;

namespace AutofacWebApiSample.Services
{
    public interface IValuesService
    {
        IEnumerable<string> FindAll();

        string Find(int id);
    }
}