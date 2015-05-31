using UnityEngine;
using System.Collections;
using System;
using Mono.Data.Sqlite;
using System.IO;
using System.Collections.Generic;
using Zeptomoby.OrbitTools;

public class DbAccess
{
 
    private SqliteConnection dbConnection;
 
    private SqliteCommand dbCommand;
 
    private SqliteDataReader reader;
 
    public DbAccess (string connectionString)
 
    {
 
        OpenDB (connectionString);
 
    }
    public DbAccess ()
	{
 
	}
 
    public void OpenDB (string connectionString)
 
    {
		try
   		 {
        	dbConnection = new SqliteConnection (connectionString);
 
       	 	dbConnection.Open ();
 
       		Debug.Log("Connected to db");
		 }
    	catch(Exception e)
    	{
       		string temp1 = e.ToString();
       		Debug.Log(temp1);
    	}
 
    }
 
    public void CloseSqlConnection () 
    {        
        dbCommand = null;

        if (reader!=null)
            reader.Close();
        reader = null;

        if (dbConnection != null) {
 
            dbConnection.Close ();
 
        }

        dbConnection = null;
 
        Debug.Log ("Disconnected from db.");
 
    }
 
    public SqliteDataReader ExecuteQuery (string sqlQuery)
 
    {
 
        dbCommand = dbConnection.CreateCommand ();
 
        dbCommand.CommandText = sqlQuery; 
        reader = dbCommand.ExecuteReader ();
 
        return reader;
 
    }
 
    public SqliteDataReader ReadFullTable (string tableName)
 
    {
 
        string query = "SELECT * FROM " + tableName;
 
        return ExecuteQuery (query);
 
    }
 
    public SqliteDataReader InsertInto (string tableName, string[] values)
 
    {
 
        string query = "INSERT INTO " + tableName + " VALUES (" + values[0];
 
        for (int i = 1; i < values.Length; ++i) {
 
            query += ", " + values[i];
 
        }
 
        query += ")";
 
        return ExecuteQuery (query);
 
    }
 
	public SqliteDataReader UpdateInto (string tableName, string []cols,string []colsvalues,string selectkey,string selectvalue)
	{
 
		string query = "UPDATE "+tableName+" SET "+cols[0]+" = "+colsvalues[0];
 
		for (int i = 1; i < colsvalues.Length; ++i) {
 
		 	 query += ", " +cols[i]+" ="+ colsvalues[i];
		}
 
		 query += " WHERE "+selectkey+" = "+selectvalue+" ";
 
		return ExecuteQuery (query);
	}
 
	public SqliteDataReader Delete(string tableName,string []cols,string []colsvalues)
	{
			string query = "DELETE FROM "+tableName + " WHERE " +cols[0] +" = " + colsvalues[0];
 
			for (int i = 1; i < colsvalues.Length; ++i) {
 
		 	    query += " or " +cols[i]+" = "+ colsvalues[i];
			}
		Debug.Log(query);
		return ExecuteQuery (query);
	}
 
    public SqliteDataReader InsertIntoSpecific (string tableName, string[] cols, string[] values)
 
    {
 
        if (cols.Length != values.Length) {
 
            throw new Exception ("columns.Length != values.Length");
 
        }
 
        string query = "INSERT INTO " + tableName + "(" + cols[0];
 
        for (int i = 1; i < cols.Length; ++i) {
 
            query += ", " + cols[i];
 
        }
 
        query += ") VALUES (" + values[0];
 
        for (int i = 1; i < values.Length; ++i) {
 
            query += ", " + values[i];
 
        }
 
        query += ")";
 
        return ExecuteQuery (query);
 
    }
 
    public SqliteDataReader DeleteContents (string tableName)
 
    {
 
        string query = "DELETE FROM " + tableName;
 
        return ExecuteQuery (query);
 
    }
 
    public SqliteDataReader CreateTable (string name, string[] col, string[] colType)
 
    {
 
        if (col.Length != colType.Length) {
 
            throw new Exception ("columns.Length != colType.Length");
 
        }
 
        string query = "CREATE TABLE " + name + " (" + col[0] + " " + colType[0];
 
        for (int i = 1; i < col.Length; ++i) {
 
            query += ", " + col[i] + " " + colType[i];
 
        }
 
        query += ")";
 
        return ExecuteQuery (query);
 
    }
 
    public SqliteDataReader SelectWhere (string tableName, string[] items, string[] col, string[] operation, string[] values)
 
    {
 
        if (col.Length != operation.Length || operation.Length != values.Length) {
 
            throw new Exception ("col.Length != operation.Length != values.Length");
 
        }
 
        string query = "SELECT " + items[0];
 
        for (int i = 1; i < items.Length; ++i) {
 
            query += ", " + items[i];
 
        }
 
        query += " FROM " + tableName + " WHERE " + col[0] + operation[0] + "'" + values[0] + "' ";
 
        for (int i = 1; i < col.Length; ++i) {
 
            query += " AND " + col[i] + operation[i] + "'" + values[0] + "' ";
 
        }
 
        return ExecuteQuery (query);
 
    }
 
}
public class SatDB : MonoBehaviour {
    DbAccess db;
    protected string db_name = "SatelliteTle.db";
    protected string sat_tle_name = "SatTle";
	// Use this for initialization
	void Awake () {        
        if (!File.Exists(db_name))
        {
            Debug.Log("SatelliteTle.db not exist, create new database.");
            db = new DbAccess("data source=" + db_name);
            db.CreateTable(sat_tle_name, new string[] { "name", "TLE1", "TLE2", "Color" }, new string[] { "text PRIMARY KEY", "text", "text", "text" });
            UpdateDBFromFile("Sat.txt");
        }
        else
        {
            Debug.Log("SatelliteTle.db already exist.");
            db = new DbAccess("data source=" + db_name);            
        }
	}

    int UpdateDBFromFile(string filename)
    {
        StreamReader fs = new StreamReader(filename);
        string line = fs.ReadLine();
        int count = 0;
        string name=null, tle1=null, tle2=null, color=null;
        while (line != null)
        {
            string[] text = line.Split('=');
            text[0] = text[0].Trim(' ');            
            if (text[0].Equals("NAME"))
            {
                if (count > 0)
                {                    
                    SqliteDataReader exist =  db.SelectWhere(sat_tle_name, 
                        new string[] {"*"}, 
                        new string[] {"name"}, 
                        new string[] {"="}, 
                        new string[] {name});
                    name = "'" + name + "'";
                    tle1 = "'" + tle1 + "'";
                    tle2 = "'" + tle2 + "'";
                    color = "'" + color + "'"; 
                    if (exist.Read())
                        db.UpdateInto(sat_tle_name,
                            new string[] { "TLE1", "TLE2", "Color" },
                            new string[] { tle1, tle2, color },
                            "name", name);
                    else
                        db.InsertInto(sat_tle_name, new string[] { name, tle1, tle2, color });
                }
                count++;
                text[1] = text[1].Trim(" \t".ToCharArray());
                text[1] = text[1].TrimStart('0');
                name = text[1].TrimStart(" \t".ToCharArray());                 
            }
            if (text[0].Equals("TLE1"))
                tle1 = text[1].Trim(" \t".ToCharArray());

            if (text[0].Equals("TLE2"))
                tle2 = text[1].Trim(" \t".ToCharArray());

            if (text[0].Equals("COLOR"))
                color = text[1].Trim(" \t".ToCharArray());
            line = fs.ReadLine();
        }
        if (count > 0)
        {
            SqliteDataReader exist = db.SelectWhere(sat_tle_name,
                new string[] { "*" },
                new string[] { "name" },
                new string[] { "=" },
                new string[] { name });
            name = "'" + name + "'";
            tle1 = "'" + tle1 + "'";
            tle2 = "'" + tle2 + "'";
            color = "'" + color + "'";
            if (exist.Read())
                db.UpdateInto(sat_tle_name,
                    new string[] { "TLE1", "TLE2", "Color" },
                    new string[] { tle1, tle2, color },
                    "name", name);
            else
                db.InsertInto(sat_tle_name, new string[] { name, tle1, tle2, color });
        }
        count++;
        return count;
    }
	
    public void GetSatInfo(string sat_name, out Tle tle, out Color c)
    {
        SqliteDataReader search = db.SelectWhere(sat_tle_name,
                        new string[] { "*" },
                        new string[] { "name" },
                        new string[] { "=" },
                        new string[] { sat_name });

        if (search.Read())
        {
            string[] color;
            string color_sh;
            tle = new Tle(sat_name, search.GetString(search.GetOrdinal("TLE1")), search.GetString(search.GetOrdinal("TLE2")));
            byte r, g, b;
            color_sh = search.GetString(search.GetOrdinal("Color"));
            color = color_sh.Split("RGB".ToCharArray());
            r = Convert.ToByte(color[1]);
            g = Convert.ToByte(color[2]);
            b = Convert.ToByte(color[3]);
            c = new Color32(r, g, b, 150);
        }
        else
            throw new Exception("GetSatInfo " + sat_name + "not found");
    }

    public string[] GetNameList()
    {
        SqliteDataReader search = db.ReadFullTable(sat_tle_name);
        List<string> result = new List<string> ();
        while (search.Read())
            result.Add(search.GetString(search.GetOrdinal("name")));

        return result.ToArray();
    }
    void OnDestroy()
    {
        db.CloseSqlConnection();
    }
}
