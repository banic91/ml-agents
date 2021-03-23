// Policy for C# training

using Unity.Barracuda;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Inference;
using Unity.MLAgents.Sensors;

namespace Unity.MLAgents.Policies
{
    internal class TrainingPolicy : IPolicy
    {
        protected TrainingModelRunner m_ModelRunner;
        ActionBuffers m_LastActionBuffer;

        int m_AgentId;

        ActionSpec m_ActionSpec;

        string m_BehaviorName;

        AgentInfo m_LastInfo;

        IReadOnlyList<TensorProxy> m_LastObservations;

        ReplayBuffer m_buffer;

        IReadOnlyList<TensorProxy> m_CurrentObservations;

        /// <inheritdoc />
        public TrainingPolicy(
            ActionSpec actionSpec,
            string behaviorName
        )
        {
            var trainer = Academy.Instance.GetOrCreateTrainer(behaviorName, actionSpec, model);
            m_ModelRunner = trainer.TrainerModelRunner;
            m_buffer = trainer.Buffer;
            m_CurrentObservations = m_ModelRunner.GetInputTensors();
            m_BehaviorName = behaviorName;
            m_ActionSpec = actionSpec;
        }

        /// <inheritdoc />
        public void RequestDecision(AgentInfo info, List<ISensor> sensors)
        {
            m_AgentId = info.episodeId;
            m_ModelRunner.PutObservations(info, sensors);
            m_ModelRunner.GetObservationTensors(m_CurrentObservations, info, sensors);

            if (m_LastObservations != null)
            {
                m_buffer.Push(m_LastInfo, m_LastObservations, m_CurrentObservations);
            }

            if (info.done == true)
            {
                m_LastObservations = null;
            }
            else
            {
                m_LastInfo = info;
                m_LastObservations = m_CurrentObservations;
            }
        }

        /// <inheritdoc />
        public ref readonly ActionBuffers DecideAction()
        {
            m_ModelRunner.DecideBatch();
            m_LastActionBuffer = m_ModelRunner.GetAction(m_AgentId);
            return ref m_LastActionBuffer;
        }

        public void Dispose()
        {
        }
    }
}
