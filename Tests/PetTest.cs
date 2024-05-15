// 1 - Bibliotecas
using Models;
using Newtonsoft.Json; // dependencia para o JsonConvert
using RestSharp;

// 2 - NameSpace
namespace Pet;

// 3 - Classe
public class PetTest
{
  // 3.1 - Atributos
  // Endereço da API
  private const string BASE_URL = "https://petstore.swagger.io/v2/";

  // 3.2 - Funções e Métodos

  // Funções de Leitura de dados a partir de um arquivo csv
  public static IEnumerable<TestCaseData> getTestData()
  {
    String caminhoMassa = @"C:\Iterasys\PetStore139\Fixtures\pets.csv";

    using var reader = new StreamReader(caminhoMassa);

    // Pula a primeira linha com os cabeçalhos
    reader.ReadLine();

    while (!reader.EndOfStream)
    {
      var line = reader.ReadLine();
      var values = line.Split(",");

      yield return new TestCaseData(int.Parse(values[0]), int.Parse(values[1]), values[2], values[3], values[4], values[5], values[6], values[7]);

    }

  }



  [Test, Order(1)]
  public void PostPetTest()
  {
    // Configura
    // instancia o objeto do tipo RestClient com o endereço da API
    var client = new RestClient(BASE_URL);

    // instancia o objeto do tipo RestRequest com o complemento de endereço
    // como "pet" e configurando o método para ser um post (inclusão)
    var request = new RestRequest("pet", Method.Post);

    // armazena o conteúdo do arquivo pet1.json na memória
    String jsonBody = File.ReadAllText(@"C:\Iterasys\PetStore139\Fixtures\Pet01.json");

    // adiciona na requisição o conteúdo do arquivo pet1.json
    request.AddBody(jsonBody);

    // Executa
    // executa a requisição conforme a configuração realizada
    // guarda o json retornado no objeto response
    var response = client.Execute(request);

    // Valida
    var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

    // Exibe o responseBody no console
    Console.WriteLine(responseBody);

    // Valide que na resposta, o status code é igual ao resultado esperado (200)
    Assert.That((int)response.StatusCode, Is.EqualTo(200));

    // Valida o PetId
    int petId = responseBody.id;
    Assert.That(petId, Is.EqualTo(20595128));

    // Valida o nome do animal na resposta
    String name = responseBody.name.ToString();
    Assert.That(name, Is.EqualTo("Kinho"));
    // OU
    // Assert.That(responseBody.name.ToString(), Is.EqualTo("Athena"));

    // Valida o status do animal na resposta
    String status = responseBody.status;
    Assert.That(status, Is.EqualTo("available"));

    // Armazenar os dados obtidos para usar os proximos testes
    Environment.SetEnvironmentVariable("petId", petId.ToString());
  }

  [Test, Order(2)]
  public void GetPetTest()
  {
    // Configura
    int petId = 20595128;         // campo de pesquisa
    string petName = "Kinho";   // resultado esperado
    string categoryName = "Dog";
    string tagsName = "vacinado";

    var client = new RestClient(BASE_URL);
    var request = new RestRequest($"pet/{petId}", Method.Get);

    // Executa
    var response = client.Execute(request);

    // Valida     
    var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
    Console.WriteLine(responseBody);

    Assert.That((int)response.StatusCode, Is.EqualTo(200));
    Assert.That((int)responseBody.id, Is.EqualTo(petId));
    Assert.That((String)responseBody.name, Is.EqualTo(petName));
    Assert.That((String)responseBody.category.name, Is.EqualTo(categoryName));
    Assert.That((String)responseBody.tags[0].name, Is.EqualTo(tagsName));
  }


  [Test, Order(3)]
  public void PutPetTest()
  {
    // Configura
    // Os dados de entrada vão formar o Body
    // vamos usar a classe de modelo
    PetModel petModel = new PetModel(); // Herança
    petModel.id = 20595128;
    petModel.category = new Category(1, "Dog");
    petModel.name = "Kinho";
    petModel.photUrls = new String[] { "" };
    petModel.tags = new Tag[] { new Tag(0, "vacinado"), new Tag(1, "vacinado") };
    petModel.status = "available";

    // Trasformar o modelo em um arquivo json
    String jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
    Console.WriteLine(jsonBody);

    var client = new RestClient(BASE_URL);
    var request = new RestRequest("pet", Method.Put);
    request.AddBody(jsonBody);

    // Executa
    var response = client.Execute(request);

    // Valida  
    var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
    Console.WriteLine(responseBody);

    Assert.That((int)response.StatusCode, Is.EqualTo(200));
    Assert.That((int)responseBody.id, Is.EqualTo(petModel.id));
    Assert.That((String)responseBody.tags[1].name, Is.EqualTo(petModel.tags[1].name));
    Assert.That((String)responseBody.status, Is.EqualTo(petModel.status));
  }



  [Test, Order(4)]
  public void DeletePetTest()
  {
    //  Configura 
    String petId = Environment.GetEnvironmentVariable("petId");
    var client = new RestClient(BASE_URL);
    var request = new RestRequest($"pet/{petId}", Method.Delete);

    // Executa
    var response = client.Execute(request);

    // Valida
    var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
    Console.WriteLine(responseBody);

    Assert.That((int)response.StatusCode, Is.EqualTo(200));
    Assert.That((int)responseBody.code, Is.EqualTo(200));
    Assert.That((String)responseBody.message, Is.EqualTo(petId));
  }

  [TestCaseSource("getTestData", new object[] { }), Order(5)]
  public void PostPetDDTest(int petId, int categoryId, String categoryName, String petName,
                             String photUrls, String tagsIds, String tagsName, String status)
  {
    // Configura
    PetModel petModel = new PetModel();
    petModel.id = petId;
    petModel.category = new Category(categoryId, categoryName);
    petModel.name = petName;
    petModel.photUrls = new String[] { photUrls };

    // Codigo para gerar as multiplas tags que o pet pode ter
    String[] tagsIdsList = tagsIds.Split(";"); // Ler
    String[] tagsNameList = tagsName.Split(";"); // Ler
    List<Tag> tagList = new List<Tag>(); // Gravar depois do for

    for (int i = 0; i < tagsIdsList.Length; i++)
    {
      int tagId = int.Parse(tagsIdsList[i]);
      String tagName = tagsNameList[i];

      Tag tag = new Tag(tagId, tagName);
      tagList.Add(tag);

    }

    petModel.tags = tagList.ToArray();
    petModel.status = status;

    // A estrutura de dados esta pronta, agora vamos serealizar
    String jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);


    // instancia o objeto do tipo RestClient com o endereço da API
    var client = new RestClient(BASE_URL);

    // instancia o objeto do tipo RestRequest com o complemento de endereço
    // como "pet" e configurando o método para ser um post (inclusão)
    var request = new RestRequest("pet", Method.Post);

    Console.WriteLine(jsonBody);
   
    // adiciona na requisição o conteúdo do arquivo pet1.json
    request.AddBody(jsonBody);

    // Executa
    // executa a requisição conforme a configuração realizada
    // guarda o json retornado no objeto response
    var response = client.Execute(request);

    // Valida
    var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

    // Exibe o responseBody no console
    Console.WriteLine(responseBody);

    // Valide que na resposta, o status code é igual ao resultado esperado (200)
    Assert.That((int)response.StatusCode, Is.EqualTo(200));

    // Valida o PetId
    Assert.That((int)responseBody.id, Is.EqualTo(petId));

    // Valida o nome do animal na resposta
    Assert.That((String)responseBody.name, Is.EqualTo(petName));
   
    // Valida o status do animal na resposta
    Assert.That((String)responseBody.status, Is.EqualTo(status));
  }
}


