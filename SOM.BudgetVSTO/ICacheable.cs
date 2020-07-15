using Jumble.ExternalCacheManager.Enums;
using SOM.BudgetVSTO.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SOM.BudgetVSTO
{
    /// <summary>
    /// Cacheable objects should not know about the database implementation class 'SQLManager'.  Cacheable objects should contain only
    /// proper query string or stored procedure for obtaining the information from the database; this information is visible to the 
    /// utility responsible for creating and maintaining the cache files (in this case, 'grabber'. 
    /// </summary>
    public interface ICacheable
    {
        public string CacheFileName { get; }
        //Holds the query or stored procedure string literal.
        public string DataRequest();
        //Identifies the base object type being serialized.
        public SOMType SOMType { get; }
        //References the ECM enum 'target database' to identify ... uh ... the target database.
        public TargetDatabase TargetDatabase { get; }
        //Returns an object of the base object type used for serialization.  Implements an internal 'SOMTypeToClass' converter.
        public object GetType();
    }
}
