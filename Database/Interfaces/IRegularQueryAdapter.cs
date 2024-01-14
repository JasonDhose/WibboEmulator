﻿namespace System.Data;
public interface IRegularQueryAdapter
{
    void AddParameter(string name, string query);
    void AddParameter(string name, int query);
    bool FindsResult();
    int GetInteger();
    DataRow GetRow();
    string GetString();
    DataTable GetTable();
    void Execute(string query);
    void SetQuery(string query);
    void Execute();
}