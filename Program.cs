using task3_SerializationAndRefluxing;
string fileDir = "D:\\Learning\\programming\\c#projects\\task3_SerializationAndRefluxing";

User user = new User("Alex1", "Merser", "female", "11.10.1994", "email@gmail.com", "1648621");
Console.WriteLine(user);

string serilizerFilePath =  user.Serialize("user.dat", fileDir);

User user2 = User.DeserializeObj(serilizerFilePath);
Console.WriteLine(user2);

// Сериализация, десериализация ,хранение пароля выполняется с его хеш-формой.
Console.WriteLine(user2.CheckPassword("1648621"));  //true


