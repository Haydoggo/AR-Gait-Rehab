public class FloatSmoothed
{
    public FloatSmoothed(float a = 0.5f)
    {
        Alpha = a;
    }

    private float _value = 0f;
    public float Value
    {
        get { return _value; }
        set { _value = _value * Alpha + value * (1f - Alpha); }
    }
    public float Alpha { get; set; } = 0.5f;
}