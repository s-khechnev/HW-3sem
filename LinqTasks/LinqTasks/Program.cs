using LinqTasks;

BusinessLogic businessLogic = new();

foreach (var item in businessLogic.GetAllAuthors())
{
    Console.WriteLine(item.ToString());
}