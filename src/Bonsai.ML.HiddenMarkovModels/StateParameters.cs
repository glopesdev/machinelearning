using System.ComponentModel;
using System;
using System.Reactive.Linq;
using System.Reflection;
using Python.Runtime;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Bonsai.ML.HiddenMarkovModels.Observations;
using Bonsai.ML.HiddenMarkovModels.Transitions;
using System.Collections.Generic;
using System.Linq;
using Bonsai.ML.Python;

namespace Bonsai.ML.HiddenMarkovModels
{
    /// <summary>
    /// Represents the state parameters of a Hidden Markov Model (HMM).
    /// </summary>
    [Combinator]
    [JsonConverter(typeof(StateParametersJsonConverter))]
    [Description("State parameters of a Hidden Markov Model (HMM).")]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class StateParameters : PythonStringBuilder
    {

        private double[] initialStateDistribution = null;
        private TransitionsModel transitions = null;
        private ObservationsModel observations = null;

        /// <summary>
        /// The initial state distribution.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("initial_state_distribution")]
        [Description("The initial state distribution.")]
        [Category("ModelStateParameters")]
        public double[] InitialStateDistribution
        {
            get => initialStateDistribution;
            set
            {
                initialStateDistribution = value;
                UpdateString();
            }
        }

        /// <summary>
        /// The transitions model.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("transition_params")]
        [Description("The transitions model.")]
        [Category("ModelStateParameters")]
        public TransitionsModel Transitions
        {
            get => transitions;
            set
            {
                transitions = value;
                UpdateString();
            }
        }

        /// <summary>
        /// The observations.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("observation_params")]
        [Description("The observations.")]
        [Category("ModelStateParameters")]
        public ObservationsModel Observations
        {
            get => observations;
            set
            {
                observations = value;
                UpdateString();
            }
        }

        /// <summary>
        /// Returns an observable sequence of <see cref="StateParameters"/> objects.
        /// </summary>
        public IObservable<StateParameters> Process()
        {
            return Observable.Return(
                new StateParameters()
                {
                    InitialStateDistribution = InitialStateDistribution,
                    Transitions = Transitions,
                    Observations = Observations
                }
            );
        }

        /// <summary>
        /// Takes an observable seqence and returns an observable sequence of <see cref="StateParameters"/> 
        /// objects that are emitted every time the input sequence emits a new element.
        /// </summary>
        public IObservable<StateParameters> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Select(source, pyObject =>
            {
                return new StateParameters()
                {
                    InitialStateDistribution = InitialStateDistribution,
                    Transitions = Transitions,
                    Observations = Observations
                };
            });
        }

        /// <summary>
        /// Transforms an observable sequence of <see cref="PyObject"/> into an observable sequence 
        /// of <see cref="StateParameters"/> objects by accessing internal attributes of the <see cref="PyObject"/>.
        /// </summary>
        public IObservable<StateParameters> Process(IObservable<PyObject> source)
        {
            return Observable.Select(source, pyObject =>
            {
                var initialStateDistributionPyObj = (double[])pyObject.GetArrayAttr("initial_state_distribution");

                var transitionsModelTypePyObj = pyObject.GetAttr<string>("transitions_model_type");
                var transitionsParamsPyObj = (Array)pyObject.GetArrayAttr("transition_params");
                var transitionsParams = (object[])transitionsParamsPyObj;

                var transitionsModelType = TransitionsModelLookup.GetFromString(transitionsModelTypePyObj);
                var transitionsClassType = TransitionsModelLookup.GetTransitionsClassType(transitionsModelType);
                var transitionsKwargsProperty = transitionsClassType.GetProperty("KwargsArray");

                object[] transitionsConstructorArgs = null;
                if (transitionsKwargsProperty is not null)
                {
                    var transitionsConstructorKeys = (string[])transitionsKwargsProperty.GetValue(null);
                    var transitionsConstructorKeysCount = transitionsConstructorKeys.Length;
                    if (transitionsConstructorKeysCount > 0)
                    {
                        transitionsConstructorArgs = new object[transitionsConstructorKeysCount];
                        var transitionsPyObj = pyObject.GetAttr("transitions");
                        for (int i = 0; i < transitionsConstructorKeysCount; i++)
                        {
                            transitionsConstructorArgs[i] = transitionsPyObj.GetArrayAttr(transitionsConstructorKeys[i]);
                        }
                    }
                }

                transitions = (TransitionsModel)Activator.CreateInstance(transitionsClassType, transitionsConstructorArgs);
                transitions.Params = transitionsParams;

                var observationsModelTypePyObj = pyObject.GetAttr<string>("observations_model_type");
                var observationsParamsPyObj = (Array)pyObject.GetArrayAttr("observation_params");
                var observationsParams = (object[])observationsParamsPyObj;

                var observationsModelType = ObservationsModelLookup.GetFromString(observationsModelTypePyObj);
                var observationsClassType = ObservationsModelLookup.GetObservationsClassType(observationsModelType);

                var observationsKwargsPyObj = (Dictionary<object, object>)pyObject.GetArrayAttr("observation_kwargs");
                var observationsConstructors = observationsKwargsPyObj.Values.ToArray();


                observations = (ObservationsModel)Activator.CreateInstance(observationsClassType,
                    observationsConstructors.Length == 0 ? null : observationsConstructors);
                observations.Params = observationsParams;

                return new StateParameters()
                {
                    InitialStateDistribution = initialStateDistributionPyObj,
                    Transitions = transitions,
                    Observations = observations
                };
            });
        }

        /// <inheritdoc/>
        protected override string BuildString()
        {
            StringBuilder.Clear();

            if (InitialStateDistribution != null)
            {
                StringBuilder.Append($"initial_state_distribution={NumpyHelper.NumpyParser.ParseArray(InitialStateDistribution)},");
            }

            if (Transitions != null)
            {
                StringBuilder.Append($"{Transitions},");
            }

            if (Observations != null)
            {
                StringBuilder.Append($"{Observations},");
            }

            if (StringBuilder.Length > 0)
            {
                StringBuilder.Remove(StringBuilder.Length - 1, 1);
            }

            return StringBuilder.ToString();
        }
    }
}
