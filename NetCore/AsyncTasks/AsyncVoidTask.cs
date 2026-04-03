#if VOIDTASK

#if GODOT
global using AsyncTaskVoid = GodotTask.GDTaskVoid;
#elif UNITY
global using AsyncTaskVoid = Cysharp.Threading.Tasks.UniTaskVoid;
#else
// Native C# does not provide a TaskVoid alternative.
// You will need to use 'VOIDTASK' preprocessor directive to differentiate the use cases.
#endif

#endif