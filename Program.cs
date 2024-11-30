//*****************************************************************************
//** 2097. Valid Arrangement of Pairs    leetcode                            **
//*****************************************************************************

#define MAX_NODES 200001

typedef struct MapNode {
    int key;
    int value;
    struct MapNode* next;
} MapNode;

typedef struct HashMap {
    MapNode* table[MAX_NODES];
} HashMap;

void initHashMap(HashMap* map) {
    memset(map->table, 0, sizeof(map->table));
}

int hash(int key) {
    return (key % MAX_NODES + MAX_NODES) % MAX_NODES;
}

void put(HashMap* map, int key, int value) {
    int index = hash(key);
    MapNode* node = map->table[index];
    while (node) {
        if (node->key == key) {
            node->value = value;
            return;
        }
        node = node->next;
    }
    MapNode* newNode = (MapNode*)malloc(sizeof(MapNode));
    newNode->key = key;
    newNode->value = value;
    newNode->next = map->table[index];
    map->table[index] = newNode;
}

int get(HashMap* map, int key, int* found) {
    int index = hash(key);
    MapNode* node = map->table[index];
    while (node) {
        if (node->key == key) {
            *found = 1;
            return node->value;
        }
        node = node->next;
    }
    *found = 0;
    return -1;
}

typedef struct {
    int* neighbors;
    int size;
    int capacity;
} AdjacencyList;

void initAdjList(AdjacencyList* list) {
    list->neighbors = NULL;
    list->size = 0;
    list->capacity = 0;
}

void addNeighbor(AdjacencyList* list, int neighbor) {
    if (list->size == list->capacity) {
        list->capacity = (list->capacity == 0) ? 4 : list->capacity * 2;
        list->neighbors = (int*)realloc(list->neighbors, list->capacity * sizeof(int));
    }
    list->neighbors[list->size++] = neighbor;
}

void freeAdjList(AdjacencyList* list) {
    free(list->neighbors);
    list->neighbors = NULL;
    list->size = list->capacity = 0;
}

int** validArrangement(int** pairs, int pairsSize, int* pairsColSize, int* returnSize, int** returnColumnSizes) {
    HashMap valueToIndex;
    initHashMap(&valueToIndex);
    int indexToValue[MAX_NODES];
    AdjacencyList adj[MAX_NODES];
    int inDegree[MAX_NODES];
    int outDegree[MAX_NODES];

    for (int i = 0; i < MAX_NODES; ++i) {
        initAdjList(&adj[i]);
    }

    memset(inDegree, 0, sizeof(inDegree));
    memset(outDegree, 0, sizeof(outDegree));

    int currentIndex = 0;

    // Map values to indices
    for (int i = 0; i < pairsSize; ++i) {
        int u = pairs[i][0];
        int v = pairs[i][1];

        int found;
        int mappedU = get(&valueToIndex, u, &found);
        if (!found) {
            mappedU = currentIndex;
            put(&valueToIndex, u, currentIndex);
            indexToValue[currentIndex++] = u;
        }

        int mappedV = get(&valueToIndex, v, &found);
        if (!found) {
            mappedV = currentIndex;
            put(&valueToIndex, v, currentIndex);
            indexToValue[currentIndex++] = v;
        }

        addNeighbor(&adj[mappedU], mappedV);
        outDegree[mappedU]++;
        inDegree[mappedV]++;
    }

    // Find starting node
    int startNode = -1;
    for (int i = 0; i < currentIndex; ++i) {
        if (outDegree[i] - inDegree[i] == 1) {
            startNode = i;
            break;
        }
    }
    if (startNode == -1) {
        for (int i = 0; i < currentIndex; ++i) {
            if (outDegree[i] > 0) {
                startNode = i;
                break;
            }
        }
    }

    if (startNode == -1) {
        *returnSize = 0;
        *returnColumnSizes = NULL;
        return NULL;
    }

    // Hierholzer's algorithm for Eulerian path or circuit
    int* route = (int*)malloc((pairsSize + 1) * sizeof(int));
    int routeIndex = 0;

    int* stack = (int*)malloc((pairsSize + 1) * sizeof(int));
    int stackTop = 0;

    stack[stackTop++] = startNode;

    while (stackTop > 0) {
        int u = stack[stackTop - 1];
        if (adj[u].size > 0) {
            int v = adj[u].neighbors[--adj[u].size];
            stack[stackTop++] = v;
        } else {
            route[routeIndex++] = u;
            stackTop--;
        }
    }

    // Reverse route
    for (int i = 0, j = routeIndex - 1; i < j; ++i, --j) {
        int temp = route[i];
        route[i] = route[j];
        route[j] = temp;
    }

    // Build result
    *returnSize = routeIndex - 1;
    *returnColumnSizes = (int*)malloc((*returnSize) * sizeof(int));
    int** result = (int**)malloc((*returnSize) * sizeof(int*));

    for (int i = 0; i < *returnSize; ++i) {
        result[i] = (int*)malloc(2 * sizeof(int));
        result[i][0] = indexToValue[route[i]];
        result[i][1] = indexToValue[route[i + 1]];
        (*returnColumnSizes)[i] = 2;
    }

    // Cleanup
    free(route);
    free(stack);
    for (int i = 0; i < MAX_NODES; ++i) {
        freeAdjList(&adj[i]);
    }

    return result;
}