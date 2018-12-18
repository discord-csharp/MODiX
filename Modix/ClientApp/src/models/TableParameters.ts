import SortParameter from './SortParameter';
import FilterParameter from './FilterParameter';

export default class TableParameters
{
    page: number = 0;
    perPage: number = 10;
    sort: SortParameter = new SortParameter();
    filters: FilterParameter[] = [];
}
