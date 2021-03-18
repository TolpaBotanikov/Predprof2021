using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;
using UnityEngine.UI;
using Vuforia;
using System.Timers;
using System.IO;

public class DataBase : MonoBehaviour
{
    public SqliteConnection dbconnection;
    private string path;
    private string markerPath;
    private string[] info;
    private TimeSpan timeNow;
    private string today;

    public void Update()
    {
        StateManager sm = TrackerManager.Instance.GetStateManager();

            IEnumerable<TrackableBehaviour> activeTrackables = sm.GetActiveTrackableBehaviours();
            foreach (TrackableBehaviour tb in activeTrackables)
            {
                infoOutput(tb.TrackableName);
            }
    }

    public void setConnection()
    {
        string fromPath = Path.Combine(Application.streamingAssetsPath, "db.bytes");
        string toPath = Path.Combine(Application.persistentDataPath, "db.bytes");

        WWW reader = new WWW(fromPath);
        while (!reader.isDone) { }

        File.WriteAllBytes(toPath, reader.bytes);

        dbconnection = new SqliteConnection("Data Source=" + toPath);
        dbconnection.Open();
    }

    public void infoOutput(string markerName)
    {
        bool isLesson = false;

        SqliteCommand cmd = new SqliteCommand();
        cmd.Connection = dbconnection;
        cmd.CommandText = $"SELECT id FROM Cabinets WHERE cabinet = '{markerName}'";
        SqliteDataReader r3 = cmd.ExecuteReader();
        string id = r3[0].ToString();
        r3.Close();

        cmd.CommandText = "SELECT * FROM Timetable WHERE cabinet = " + id;
        SqliteDataReader r = cmd.ExecuteReader();

        while (r.Read())
        {
            info = new string[] { r[1].ToString(), r[2].ToString(), r[3].ToString(), r[4].ToString(), r[5].ToString() };
            timeNow = DateTime.Now.TimeOfDay;
            today = DateTime.Now.DayOfWeek.ToString();

            SqliteCommand cmd2 = new SqliteCommand();
            cmd2.Connection = dbconnection;
            cmd2.CommandText = "SELECT time1, time2 FROM Time WHERE id = " + info[1];
            SqliteDataReader r2 = cmd2.ExecuteReader();
            string time1 = r2[0].ToString();
            string time2 = r2[1].ToString();
            r2.Close();

            string[] arr_time1 = time1.Split();
            int x1 = int.Parse(arr_time1[0]);
            int x2 = int.Parse(arr_time1[1]);
            int x3 = int.Parse(arr_time1[2]);

            string[] arr_time2 = time2.Split();
            int y1 = int.Parse(arr_time2[0]);
            int y2 = int.Parse(arr_time2[1]);
            int y3 = int.Parse(arr_time2[2]);

            cmd2.CommandText = "SELECT day FROM Days WHERE id = " + info[0];
            r2 = cmd2.ExecuteReader();
            string day = r2[0].ToString();
            r2.Close();

            if (timeNow > new TimeSpan(x1, x2, x3) && timeNow < new TimeSpan(y1, y2, y3) && today == day && isLesson is false)
            {
                cmd2.CommandText = "SELECT class FROM Classes WHERE id = " + info[2];
                r2 = cmd2.ExecuteReader();
                string classs = r2[0].ToString();
                r2.Close();

                cmd2.CommandText = "SELECT lesson, teacher FROM Lessons WHERE id = " + info[3];
                r2 = cmd2.ExecuteReader();
                string lesson = r2[0].ToString();
                string teacher = r2[1].ToString();
                r2.Close();

                isLesson = true;
                GameObject.Find(markerName + "/Canvas/Image/Text").GetComponent<Text>().text = $"Сейчас здесь идёт {lesson} у {classs}. Преподаватель: {teacher}.";
            }
        }

        if (isLesson is false)
        {

            GameObject.Find(markerName + "/Canvas/Image/Text").GetComponent<Text>().text = "Сейчас здесь не проходит урок.";
        }
        r.Close();
    }
}
