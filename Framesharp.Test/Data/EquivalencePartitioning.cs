using System;
using System.Collections;
using System.Collections.Generic;
using Framesharp.Data;
using Framesharp.Data.Interfaces;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace Framesharp.Test.Data
{
    public class EquivalencePartitioning
    {
        public readonly IStatelessSessionProvider SessionContainer;

        private int _retrieveTryCount;

        public EquivalencePartitioning(IStatelessSessionContainer sessionContainer)
        {
            SessionContainer = sessionContainer as IStatelessSessionProvider;

            _retrieveTryCount = 0;
        }

        public T GetRandom<T>() where T : class
        {
            return GetRandom<T>(default(Dictionary<string,object>));
        }

        public T GetRandom<T>(IDictionary filterDictionary) where T : class
        {
            ICriteria criteria = SessionContainer.GetSession().CreateCriteria<T>();

            if (filterDictionary != null && filterDictionary.Count > 0) criteria.Add(Restrictions.AllEq(filterDictionary));

            return GetRandom<T>(criteria);
        }

        public T GetRandom<T>(ICriteria criteria) where T : class
        {
            if (_retrieveTryCount.Equals(7))
                throw new NHibernate.QueryException(string.Format("No results for entity {0} where retrieved. Maybe the table is empty.", typeof(T).Name));

            try
            {
                return criteria.AddOrder(new RandomOrder()).SetMaxResults(1).UniqueResult<T>();
            }
            catch (NHibernate.ObjectNotFoundException)
            {
                _retrieveTryCount++;

                return GetRandom<T>();
            }
        }

        public T GetById<T>(object id) where T : class
        {
            ICriteria criteria = SessionContainer.GetSession().CreateCriteria<T>();

            return criteria.Add(Restrictions.Eq(string.Format("{0}Id", typeof (T).Name), id)).UniqueResult<T>();
        }
    }

    public class RandomOrder : Order
    {
        public RandomOrder() : base("", true) { }

        public override SqlString ToSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery)
        {
            return new SqlString("newid()");
        }
    }
}
