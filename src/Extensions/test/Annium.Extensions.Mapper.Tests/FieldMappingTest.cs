// using System;
// using System.Collections.Generic;
// using Annium.Testing;
// using NodaTime;

// namespace Annium.Extensions.Mapper.Tests
// {
//     public class FieldMappingTest
//     {
//         [Fact]
//         public void ConfigurationMapping_Works()
//         {
//             // arrange
//             var mapper = GetMapper();
//             var date = new DateTime(2000, 10, 7).ToUniversalTime();
//             var instant = Instant.FromDateTimeUtc(new DateTime(2002, 6, 17).ToUniversalTime());
//             var value = new Payload[] { new ImagePayload("img", date), new LinkPayload { Link = "lnk", Created = instant } };

//             // act
//             var result = mapper.Map<List<Model>>(value);

//             // assert
//             result.Has(2);
//             result.At(0).As<ImageModel>().Image.IsEqual("img");
//             result.At(0).As<ImageModel>().Created.ToDateTimeUtc().IsEqual(date);
//             result.At(1).As<LinkModel>().Link.IsEqual("lnk");
//             result.At(1).As<LinkModel>().Created.IsEqual(instant.ToDateTimeUtc());
//         }

//         private IMapper GetMapper()
//         {
//             var cfg = new MapperConfiguration();
//             cfg.Map<DateTime, Instant>(d => Instant.FromDateTimeUtc(d.ToUniversalTime()));
//             cfg.Map<Instant, DateTime>(i => i.ToDateTimeUtc());

//             var builder = new MapBuilder(cfg, TypeResolverAccessor.TypeResolver, new Repacker());

//             return new Mapper(builder);
//         }

//         private class A
//         {
//             public string Text { get; set; }
//         }

//         private class B
//         {
//             public int Ignored { get; set; }

//             public string LowerText { get; set; }
//         }
//     }
// }