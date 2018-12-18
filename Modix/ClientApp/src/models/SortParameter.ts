import { SortDirection } from './SortDirection';

export default class SortParameter
{
    field: string = "";
    direction: SortDirection = SortDirection.Descending;
}
