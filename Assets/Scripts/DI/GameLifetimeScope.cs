using TapMatch.GameLogic;
using TapMatch.Runtime.Board;
using TapMatch.Runtime.Config;
using TapMatch.Runtime.Input;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace TapMatch.Runtime.DI
{
    /// <summary>
    /// VContainer LifetimeScope that wires up all dependencies for the game.
    /// This is the composition root — the single place where the object graph is assembled.
    ///
    /// Dependency graph:
    ///   GameConfig (ScriptableObject, serialized)
    ///     → GridModel, IRandomProvider, MatchFinder, GridCollapser, GridFiller
    ///       → GameController
    ///     → BoardView (MonoBehaviour, scene)
    ///     → InputHandler (MonoBehaviour, scene)
    ///       → BoardPresenter (MonoBehaviour, scene)
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private BoardView boardView;
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private BoardPresenter boardPresenter;

        protected override void Configure(IContainerBuilder builder)
        {
            // Configuration
            builder.RegisterInstance(gameConfig);

            // Pure game logic (no Unity dependencies)
            builder.Register<IRandomProvider, SystemRandomProvider>(Lifetime.Singleton);
            builder.Register<MatchFinder>(Lifetime.Singleton);
            builder.Register<GridCollapser>(Lifetime.Singleton);
            builder.Register<GridFiller>(Lifetime.Singleton);

            builder.Register<GridModel>(Lifetime.Singleton)
                .WithParameter("rows", gameConfig.Rows)
                .WithParameter("columns", gameConfig.Columns);

            builder.Register<GameController>(Lifetime.Singleton)
                .WithParameter("colorCount", gameConfig.ColorCount);

            // Unity MonoBehaviour components (scene references)
            // RegisterComponent auto-calls [Inject] methods on these MonoBehaviours
            builder.RegisterComponent(boardView);
            builder.RegisterComponent(inputHandler);
            builder.RegisterComponent(boardPresenter);

            // Interface → implementation mapping
            builder.RegisterInstance<IBoardView>(boardView);

            // Entry point — VContainer calls Start() after all injections are resolved
            builder.RegisterEntryPoint<GameStartup>();
        }
    }
}
