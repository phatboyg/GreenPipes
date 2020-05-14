namespace GreenPipes.Agents
{
    public static class AgentExtensions
    {
        /// <summary>
        /// Returns true if the Agent is Ready (completed, faulted, or canceled)
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public static bool IsReady(this IAgent agent)
        {
            return agent.Ready.IsCompleted;
        }

        /// <summary>
        /// Returns true if the Agent is Completed (completed, faulted, or canceled)
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public static bool IsCompleted(this IAgent agent)
        {
            return agent.Completed.IsCompleted;
        }
    }
}
