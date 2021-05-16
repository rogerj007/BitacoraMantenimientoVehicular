﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace BitacoraMantenimientoVehicular.Bot.Helpers
{
    public static class HelperExtension
    {
        public static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate
                {
                    using (partition) while (partition.MoveNext()) await body(partition.Current);
                }));
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body)
        {
            List<Exception> exceptions = null;
            foreach (var item in source)
            {
                try { await body(item); }
                catch (Exception exc)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(exc);
                }
            }
            if (exceptions != null)
                throw new AggregateException(exceptions);
        }

        public static StringBuilder SqlExceptionMessage(this SqlException ex)
        {
            var sqlErrorMessages = new StringBuilder("Sql Exception:\n");

            foreach (SqlError error in ex.Errors)
            {
                sqlErrorMessages.AppendFormat("Mesage: {0}\n", error.Message)
                    .AppendFormat("Severity level: {0}\n", error.Class)
                    .AppendFormat("State: {0}\n", error.State)
                    .AppendFormat("Number: {0}\n", error.Number)
                    .AppendFormat("Procedure: {0}\n", error.Procedure)
                    .AppendFormat("Source: {0}\n", error.Source)
                    .AppendFormat("LineNumber: {0}\n", error.LineNumber)
                    .AppendFormat("Server: {0}\n", error.Server)
                    .AppendLine(new string('-', error.Message.Length + 7));

            }
            return sqlErrorMessages;
        }

        public static bool IsFileLocked(this FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                stream?.Close();
            }

            //file is not locked
            return false;
        }

        public static string ToMessageAndCompleteStacktrace(this Exception exception)
        {
            var e = exception;
            var s = new StringBuilder();
            while (e != null)
            {
                s.AppendLine("Exception type: " + e.GetType().FullName);
                s.AppendLine("Message       : " + e.Message);
                s.AppendLine(@"Stacktrace:");
                s.AppendLine(e.StackTrace);
                e = e.InnerException;
            }
            return s.ToString();
        }
    }
}
