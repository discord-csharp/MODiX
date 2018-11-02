export default class Deserializer
{
    static getNew<T>(testType: new () => T, input: any) : T
    {
        let instance = new testType() as any;

        for (let prop in input)
        {
            instance[prop] = input[prop];
        }

        return instance;
    }
}