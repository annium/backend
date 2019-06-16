using System;
using System.Collections.Generic;
using Annium.Extensions.DependencyInjection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Extensions.Mapper.Tests
{
    public class ConfigurationTest
    {
        [Fact]
        public void ConfigurationMapping_Works()
        {
            // arrange
            var mapper = GetMapper();
            var date = new DateTime(2000, 10, 7).ToUniversalTime();
            var instant = Instant.FromDateTimeUtc(new DateTime(2002, 6, 17).ToUniversalTime());
            var value = new Payload[] { new ImagePayload("img", date), new LinkPayload { Link = "lnk", Created = instant } };

            // act
            var result = mapper.Map<List<Model>>(value);

            // assert
            result.Has(2);
            result.At(0).As<ImageModel>().Image.IsEqual("img");
            result.At(0).As<ImageModel>().Created.ToDateTimeUtc().IsEqual(date);
            result.At(1).As<LinkModel>().Link.IsEqual("lnk");
            result.At(1).As<LinkModel>().Created.IsEqual(instant.ToDateTimeUtc());
        }

        private IMapper GetMapper() => new ServiceCollection()
            .AddMapper(new ServiceCollection().AddMapperConfiguration(ConfigureMapping).BuildServiceProvider())
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();

        private void ConfigureMapping(MapperConfiguration cfg)
        {
            cfg.Map<DateTime, Instant>(d => Instant.FromDateTimeUtc(d.ToUniversalTime()));
            cfg.Map<Instant, DateTime>(i => i.ToDateTimeUtc());
        }

        private abstract class Payload { }

        private class ImagePayload : Payload
        {
            public string Image { get; }

            public DateTime Created { get; }

            public ImagePayload(string image, DateTime created)
            {
                Image = image;
                Created = created;
            }
        }

        private class LinkPayload : Payload
        {
            public string Link { get; set; }

            public Instant Created { get; set; }
        }

        private abstract class Model { }

        private class ImageModel : Model
        {
            public string Image { get; set; }

            public Instant Created { get; set; }
        }

        private class LinkModel : Model
        {
            public string Link { get; }

            public DateTime Created { get; }

            public LinkModel(string link, DateTime created)
            {
                Link = link;
                Created = created;
            }
        }
    }
}