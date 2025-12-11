import "@typespec/http";

using Http;

@service(#{
  title: "Store API",
})
namespace StoreService;

model Store {
  id: string;
  name: string;
  address: Address;
}

model Address {
  street: string;
  city: string;
  state: string;
  zipCode: string;
}

@route("/stores")
interface Stores {
  @get list(@query filter?: string): Store[];
  @get read(@path id: string): Store;
  @post create(@body store: Store): Store;
  @put update(@path id: string, @body store: Store): Store;
  @delete remove(@path id: string): void;
}
