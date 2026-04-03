#if GODOT
global using BoolTask = GodotTask.GDTask<bool>;
#elif UNITY
global using BoolTask = Cysharp.Threading.Tasks.UniTask<bool>;
#else
global using BoolTask = System.Threading.Tasks.Task<bool>;
#endif