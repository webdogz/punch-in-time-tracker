namespace PunchIn.Extensions
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    public class BindableStaticResource : StaticResourceExtension
    {
        private static readonly DependencyProperty DummyProperty;

        static BindableStaticResource()
        {
            DummyProperty = DependencyProperty.RegisterAttached("Dummy",
                                                                typeof(Object),
                                                                typeof(DependencyObject),
                                                                new UIPropertyMetadata(null));
        }

        public Binding MyBinding { get; set; }

        public BindableStaticResource()
        {
        }

        public BindableStaticResource(Binding binding)
        {
            MyBinding = binding;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            var targetObject = (FrameworkElement)target.TargetObject;

            MyBinding.Source = targetObject.DataContext;
            var DummyDO = new DependencyObject();
            BindingOperations.SetBinding(DummyDO, DummyProperty, MyBinding);

            ResourceKey = DummyDO.GetValue(DummyProperty);

            return base.ProvideValue(serviceProvider);
        }
    }
}