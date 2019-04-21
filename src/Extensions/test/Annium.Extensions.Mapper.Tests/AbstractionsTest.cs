using System.Collections.Generic;
using Annium.Testing;

namespace Annium.Extensions.Mapper.Tests
{
    public class AbstractionsTest
    {
        [Fact]
        public void ToArray_Works()
        {
            // arrange
            var mapper = GetMapper();
            var value = new Payload[] { new ImagePayload("img"), new LinkPayload { Link = "lnk" } };

            // act
            var result = mapper.Map<List<Model>>(value);

            // assert
            result.Has(2);
            result.At(0).As<ImageModel>().Image.IsEqual("img");
            result.At(1).As<LinkModel>().Link.IsEqual("lnk");
        }

        private IMapper GetMapper()
        {
            var builder = new MapBuilder(new MapperConfiguration(), TypeResolverAccessor.TypeResolver, new Repacker());

            return new Mapper(builder);
        }

        private abstract class Payload { }

        private class ImagePayload : Payload
        {
            public string Image { get; }

            public ImagePayload(string image)
            {
                Image = image;
            }
        }

        private class LinkPayload : Payload
        {
            public string Link { get; set; }
        }

        private abstract class Model { }

        private class ImageModel : Model
        {
            public string Image { get; set; }
        }

        private class LinkModel : Model
        {
            public string Link { get; }

            public LinkModel(string link)
            {
                Link = link;
            }
        }
    }
}