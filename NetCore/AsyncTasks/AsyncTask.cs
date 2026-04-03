#if GODOT
global using AsyncTask = GodotTask.GDTask;
#elif UNITY
global using AsyncTask = Cysharp.Threading.Tasks.UniTask;
#else
global using AsyncTask = System.Threading.Tasks.Task;
#endif