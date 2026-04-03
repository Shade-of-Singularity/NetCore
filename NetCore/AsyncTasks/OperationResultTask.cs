#if GODOT
global using OperationResultTask = GodotTask.GDTask<NetCore.OperationResult>;
#elif UNITY
global using OperationResultTask = Cysharp.Threading.Tasks.UniTask<NetCore.OperationResult>;
#else
global using OperationResultTask = System.Threading.Tasks.Task<NetCore.OperationResult>;
#endif