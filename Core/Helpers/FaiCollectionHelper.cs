﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Enums;
using Core.ViewModels.Database.FaiCollection;
using Dapper;

namespace Core.Helpers
{
    public static class FaiCollectionHelper
    {
        public static void SetFaiValues(this IFaiCollection faiCollection, IDictionary<string, double> values, DateTime inspectionTime, int cavityNo, string result)
        {
            // Assign meta data
            faiCollection.Result = result;
            faiCollection.Cavity = cavityNo;
            faiCollection.InspectionTime = inspectionTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            
            // Assign fai values
            var faiProps = faiCollection.GetType().GetProperties().Where(prop => prop.Name.Contains("Fai"));

            foreach (var faiProp in faiProps)
            {
                faiProp.SetValue(faiCollection, values[ToSingleUnderScore(faiProp.Name)]);
            }
        }

        private static string ToSingleUnderScore(string name)
        {
            return name.Contains('_') ? name.Remove(name.IndexOf('_'), 1) : name;
        }
        

        public static async Task<IList<TFaiCollection>> SelectByIntervalAsync<TFaiCollection>(ProductType productType, string connectionString,
            DateTime timeStart, DateTime timeEnd) where TFaiCollection : IFaiCollection
        {
            var query = SelectByIntervalQueries[productType];
            var timeStartText = timeStart.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var timeEndText = timeEnd.ToString("yyyy-MM-dd HH:mm:ss.fff");
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {

                var output = await connection.QueryAsync<TFaiCollection>(query,
                        new {TimeStart = timeStartText, TimeEnd = timeEndText});

                return output.ToList();
            }
        }

        /// <summary>
        /// Insert into local database
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="connectionString"></param>
        public static void Insert(this IFaiCollection collection, string connectionString)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                var productType = collection is FaiCollectionMtm ? ProductType.Mtm : ProductType.Alps;
                var insertQuery = InsertQueries[productType];
                connection.Execute(insertQuery, collection);
            }
        }
        
        
        private static readonly Dictionary<ProductType, string> SelectByIntervalQueries = new Dictionary<ProductType, string>()
        {
            [ProductType.Mtm] = "dbo.spSelectFaiCollectionByIntervalMtm @TimeStart, @TimeEnd",
            [ProductType.Alps] ="dbo.spSelectFaiCollectionByIntervalAlps @TimeStart, @TimeEnd",
        };
        
        private static readonly Dictionary<ProductType, string> InsertQueries = new Dictionary<ProductType, string>()
        {
            [ProductType.Mtm] = "dbo.spInsertMtm @InspectionTime, @Cavity, @Result, @FAI1__1A,@FAI1__1B,@FAI1__1C,@FAI1__2A,@FAI1__2B,@FAI1__2C,@FAI2__1A,@FAI2__1B,@FAI2__1C,@FAI2__2A,@FAI2__2B,@FAI2__2C,@FAI9__1,@FAI9__2,@FAI10__1,@FAI10__2,@FAI23__A,@FAI23__C,@FAI24__A,@FAI24__B,@FAI24__C,@FAI33__1,@FAI33__2,@FAI34__1,@FAI34__2,@FAI43__A,@FAI43__B,@FAI43__C,@FAI46__A,@FAI46__B,@FAI46__C,@FAI__52,@FAI11__1,@FAI11__2,@FAI11__3,@FAI11__4,@FAI11__5,@FAI11__6,@FAI11__7,@FAI11__8,@FAI16__1,@FAI16__2,@FAI17__1,@FAI17__2,@FAI17__3,@FAI17__4,@FAI18C,@FAI18M,@FAI19__1,@FAI19__2,@FAI19__3,@FAI19__4,@FAI19__5,@FAI19__6,@FAI19__7,@FAI19__8,@FAI19__9,@FAI19__10,@FAI19__11,@FAI19__12,@FAI20__1,@FAI20__2,@FAI20__3,@FAI20__4,@FAI21,@FAI22",
            [ProductType.Alps] ="dbo.spInsertAlps @InspectionTime, @Cavity, @Result, @FAI1__1A,@FAI1__1B,@FAI1__1C,@FAI1__2A,@FAI1__2B,@FAI1__2C,@FAI2__1A,@FAI2__1B,@FAI2__1C,@FAI2__2A,@FAI2__2B,@FAI2__2C,@FAI9__1,@FAI9__2,@FAI10__1,@FAI10__2,@FAI23__A,@FAI23__C,@FAI24__A,@FAI24__B,@FAI24__C,@FAI33__1,@FAI33__2,@FAI34__1,@FAI34__2,@FAI43__A,@FAI43__B,@FAI43__C,@FAI46__A,@FAI46__B,@FAI46__C,@FAI__52,@FAI11__1,@FAI11__2,@FAI11__3,@FAI11__4,@FAI11__5,@FAI11__6,@FAI11__7,@FAI11__8,@FAI16__1,@FAI16__2,@FAI17__1,@FAI17__2,@FAI17__3,@FAI17__4,@FAI18C,@FAI18M,@FAI19__1,@FAI19__2,@FAI19__3,@FAI19__4,@FAI19__5,@FAI19__6,@FAI19__7,@FAI19__8,@FAI19__9,@FAI19__10,@FAI19__11,@FAI19__12,@FAI20__1,@FAI20__2,@FAI20__3,@FAI20__4,@FAI21,@FAI22",
        };
        
    }
}