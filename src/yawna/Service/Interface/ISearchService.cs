using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace yawna.Service.Interface
{
  public interface ISearchService
  {
    IObservable<string> Message { get; }

    Task<List<string>> QueryAsync(string filter);
  }
}