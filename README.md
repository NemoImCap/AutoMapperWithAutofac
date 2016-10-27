# Configuring AutoMapper to fulfil ITypeConverter<,> constructor dependecies with Autofac

First time working with Autofac to inject AutoMapper's `IMapper` interface into classes that have a object mapping requirement.  I have made some progress getting the various dependencies added to AutoMapper's register using Assembly Scanning:

<!-- language: lang-cs -->

    builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
        .AsClosedTypesOf(typeof(ITypeConverter<,>))
        .AsImplementedInterfaces();
    builder.RegisterAssemblyTypes(typeof(AutoMapperExtensions).Assembly)
        .AssignableTo<Profile>().As<Profile>();

    builder.Register(context => {
        var profiles = context.Resolve<IEnumerable<Profile>>();
        return new MapperConfiguration(x => {
            foreach (var profile in profiles) x.AddProfile(profile);
        });
    }).SingleInstance().AutoActivate().AsSelf();

    builder.Register(context => {
        var componentContext = context.Resolve<IComponentContext>();
        var config = componentContext.Resolve<MapperConfiguration>();
        return config.CreateMapper();
    }).As<IMapper>();

This works perfectly for an `ITypeConverter<,>` that dosn't have any injected dependencies:

<!-- language: lang-cs -->

    public class SourceToDestinationTypeConverter : ITypeConverter<SourceModel, DestinationModel> {
        public DestinationModel Convert(SourceModel source, DestinationModel destination, ResolutionContext context) {
            if (source.Items == null) {
                return null;
            }

            return new DestinationModel {
                FirstItem = source.Items.FirstOrDefault(),
                LastItem = source.Items.LastOrDefault()
            };
        }
    }

However from the moment I add a dependency, in this contrived example, n validator:

<!-- language: lang-cs -->

    public class SourceToDestinationTypeConverter : ITypeConverter<SourceModel, DestinationModel> {
        private readonly IValidator<SourceModel> _validator;

        public SourceToDestinationTypeConverter(IValidator<SourceModel> validator) {
            _validator = validator;
        }

        public DestinationModel Convert(SourceModel source, DestinationModel destination, ResolutionContext context) {
            if (!_validator.Validate(source)) return null;

            return new DestinationModel {
                FirstItem = source.Items.FirstOrDefault(),
                LastItem = source.Items.LastOrDefault()
            };
        }
    }

The following exception is thrown:

> `Application.TypeConverters.SourceToDestinationTypeConverter` needs to have a constructor with 0 args or only optional args

It seems clear to me that AutoMapper **needs to be told to use Autofac** to fulfil the dependencies however I haven't been able to find out how to tell it to do so.

The full solution is [available on GitHub][github-repo] if further clarification of the error is required.

  [github-repo]: https://github.com/RichardSlater/AutoMapperWithAutofac
