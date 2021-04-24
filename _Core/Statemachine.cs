using System.Collections;
using System.Collections.Generic;

public interface IState
{
    void OnInit(StateMachine statemachine, TypeMap data);
    void OnEnter(StateMachine statemachine, TypeMap data);
    void OnUpdate(StateMachine statemachine, TypeMap data, float delta, float state_time);
    void OnExit(StateMachine statemachine, TypeMap data);
}

public abstract class State: IState
{
    public virtual void OnInit(StateMachine stateMachine, TypeMap data) {}
    public virtual void OnEnter(StateMachine stateMachine, TypeMap data) {}
    public virtual void OnUpdate(StateMachine stateMachine, TypeMap data, float delta, float state_time) { }
    public virtual void OnExit(StateMachine stateMachine, TypeMap data) {}
}

public abstract class SubState : IState
{
    public StateMachine substate_statemachine {get; private set;} = new StateMachine();

    void IState.OnUpdate(StateMachine statemachine, TypeMap data, float delta, float state_time)
    {
        OnUpdate(statemachine, data, delta, state_time);
        substate_statemachine.Update(data, delta);
    }

    public virtual void OnInit(StateMachine stateMachine, TypeMap data) {}
    public virtual void OnEnter(StateMachine stateMachine, TypeMap data) {}
    public virtual void OnUpdate(StateMachine stateMachine, TypeMap data, float delta, float state_time) { }
    public virtual void OnExit(StateMachine stateMachine, TypeMap data) {}
}

public class StateMachine: IEnumerable<IState>
{
    Dictionary<int, IState> _states = new Dictionary<int, IState>();

    public IState previous {get; private set;}
    public IState current {get; private set;}
    public IState next {get; private set;}
    bool init;
    float state_time = 0;

    public void Update(TypeMap data, float delta)
    {
        state_time += delta;
        if (next != null)
        {
            current?.OnExit(this, data);
            state_time = 0;
            previous = current;
            current = next;
            next = null;
            if (init)
            {
                init = false;
                current?.OnInit(this, data);
            }
            current?.OnEnter(this, data);
            return;
        }
        current?.OnUpdate(this, data, delta, state_time);
    }

    public void Change<New_State>() where New_State : IState, new()
    {
        if (next != null) return;
        if (!_states.TryGetValue(TypeID.Get<New_State>(), out var next_state))
        {
            _states[TypeID.Get<New_State>()] = next_state = new New_State();
            init = true;
        }
        next = next_state;
    }

    public override string ToString()
    {
        return $"{GetType()} : {current}";
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach(var entry in _states)
        {
            yield return entry.Value;
        }
    }

    IEnumerator<IState> IEnumerable<IState>.GetEnumerator()
    {
        foreach(var state in _states.Values)
        {
            if (state is SubState subState)
            {
                foreach(IState subState_state in subState.substate_statemachine)
                {
                    yield return subState_state;
                }
            }
            yield return state;
        }
    }
}